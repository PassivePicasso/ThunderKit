using AssetsExporter;
using AssetsExporter.Collection;
using AssetsExporter.Meta;
using AssetsExporter.YAML;
using AssetsExporter.YAMLExporters.Info;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Config.Common
{
    [Serializable]
    public class ImportProjectSettings : OptionalExecutor
    {
        private static readonly Dictionary<AssetClassID, string> projectSettingAssetToFileName = new Dictionary<AssetClassID, string>()
        {
            [AssetClassID.PhysicsManager] = "DynamicsManager",
            [AssetClassID.NavMeshProjectSettings] = "NavMeshAreas",
            [AssetClassID.PlayerSettings] = "ProjectSettings",
        };
        private static readonly HashSet<AssetClassID> ignoreTypesOnExport = new HashSet<AssetClassID>
        {
            AssetClassID.PreloadData,
            AssetClassID.AssetBundle,
            AssetClassID.BuildSettings,
            AssetClassID.DelayedCallManager,
            AssetClassID.MonoManager,
            AssetClassID.ResourceManager,
            AssetClassID.RuntimeInitializeOnLoadManager,
            AssetClassID.ScriptMapper,
            AssetClassID.StreamingManager,
            AssetClassID.MonoScript,
        };


        public override int Priority => Constants.Priority.ProjectSettingsImport;
        public override string Description => "Import ProjectSettings from games with globalgamemanagers";
        public long IncludedSettings;
        private AssetsManager assetsManager;
        private YAMLExportManager exportManager;
        private PPtrExporterInfo pptrExporterInfo;
        private Dictionary<string, object> exporterInfo;
        private string outputProjectSettingsDirectory;

        public override bool Execute()
        {
            if (IncludedSettings == 0) return true;

            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var classDataPath = ClassDataManager.GetClassDataPath();
            if (string.IsNullOrEmpty(classDataPath) || !File.Exists(classDataPath))
            {
                Debug.LogError("[ThunderKit] Skipping ProjectSettings import: no class data (classdata.tpk) is available for this Unity version.");
                return true;
            }

            var unityVersion = Application.unityVersion;
            var globalGameManagersPath = Path.Combine(settings.GameDataPath, "globalgamemanagers");
            var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "ImportedProjectSettings");

            try
            {
                ExportProjectSettings(classDataPath, globalGameManagersPath, outputDirectory, unityVersion);
            }
            catch (UnsupportedClassDataException e)
            {
                Debug.LogError($"[ThunderKit] Skipping ProjectSettings import: {e.Message}");
                return true;
            }

            var includedSettings = (IncludedSettings)IncludedSettings;
            var importedSettings = new List<string>();
            foreach (IncludedSettings include in (IncludedSettings[])Enum.GetValues(typeof(IncludedSettings)))
            {
                if (include == 0) continue;
                if (!includedSettings.HasFlag(include)) continue;

                string settingName = $"{include}.asset";
                string settingPath = Path.Combine("ProjectSettings", settingName);
                string tempSettingPath = Path.Combine(outputProjectSettingsDirectory, settingName);
                if (!File.Exists(tempSettingPath)) continue;

                File.Copy(tempSettingPath, settingPath, true);
                //Update times as necessary to trigger unity import
                File.SetLastWriteTime(settingPath, DateTime.Now);
                File.SetLastAccessTime(settingPath, DateTime.Now);
                File.SetCreationTime(settingPath, DateTime.Now);
                importedSettings.Add(settingPath);
            }

            if (importedSettings.Count > 0) 
                AssetDatabase.ImportAsset(importedSettings[0]);

            var escape = false;
            while (EditorApplication.isUpdating && !escape)
            {
                var x = escape;
            }
            return true;
        }


        // Raised when the class data (tpk) cannot be used to interpret the game data at
        // all: an unparseable Unity version or a tpk that lists no type versions. The
        // production import treats this as "skip ProjectSettings import" rather than a
        // hard failure; tests use it to distinguish a total failure from the expected
        // per-setting skips handled inside ExportGlobalGameManagers.
        internal class UnsupportedClassDataException : Exception
        {
            public UnsupportedClassDataException(string message) : base(message) { }
        }

        // Core of the import: resolve a class database from the tpk for the running Unity
        // version, load the game's globalgamemanagers, and export each contained setting
        // to a YAML .asset under outputDirectory. Returns the paths actually written.
        //
        // Decoupled from ThunderKitSettings/AssetDatabase so it can be exercised directly
        // against committed fixtures (see ImportProjectSettingsTests). Throws
        // UnsupportedClassDataException for the two unrecoverable cases; individual
        // settings whose type data is missing are skipped (logged) rather than thrown.
        internal List<string> ExportProjectSettings(string classDataPath, string globalGameManagersPath, string outputDirectory, string unityVersion)
        {
            outputProjectSettingsDirectory = outputDirectory;

            pptrExporterInfo = new PPtrExporterInfo
            {
                storeFoundCollections = true,
            };
            exporterInfo = new Dictionary<string, object>
            {
                [nameof(PPtrExporterInfo)] = pptrExporterInfo,
            };

            assetsManager = new AssetsManager();
            assetsManager.LoadClassPackage(classDataPath);

            if (!ClassDataManager.TryParseUnityVersion(unityVersion, out var major, out var minor, out var patch))
                throw new UnsupportedClassDataException($"could not parse Unity version '{unityVersion}'.");

            // The tpk rarely lists every Unity version. Discover the versions it does
            // contain and load the class database for the closest one (newest at or
            // before the running version) rather than requiring an exact match.
            var availableVersions = assetsManager.ClassPackage?.TpkTypeTree?.Versions;
            var resolvedVersion = ClassDataManager.SelectBestVersion(availableVersions, major, minor, patch);
            if (resolvedVersion == null)
                throw new UnsupportedClassDataException("class data (classdata.tpk) contains no type versions.");

            if (resolvedVersion.major != major || resolvedVersion.minor != minor || resolvedVersion.patch != patch)
            {
                Debug.LogWarning($"[ThunderKit] No exact class data for Unity {unityVersion}; using the closest available type data " +
                    $"({resolvedVersion.major}.{resolvedVersion.minor}.{resolvedVersion.patch}). " +
                    "Individual settings may fail to import if their type information changed between these versions.");
            }

            assetsManager.LoadClassDatabaseFromPackage(resolvedVersion);
            exportManager = YAMLExportManager.CreateDefault();

            var globalGameManagersFile = assetsManager.LoadAssetsFile(globalGameManagersPath, true);

            return ExportGlobalGameManagers(globalGameManagersFile, new UnityVersion(unityVersion));
        }

        private List<string> ExportGlobalGameManagers(AssetsFileInstance globalGameManagersFile, UnityVersion unityVersion)
        {
            var exportedFiles = new List<string>();
            foreach (var info in globalGameManagersFile.file.AssetInfos)
            {
                var type = (AssetClassID)info.TypeId;
                if (ignoreTypesOnExport.Contains(type))
                {
                    continue;
                }

                if (!projectSettingAssetToFileName.TryGetValue(type, out var fileName))
                {
                    fileName = Enum.GetName(typeof(AssetClassID), info.TypeId);
                }

                // A given setting type may be absent from the class data when the tpk
                // does not fully cover the running Unity version (see ClassDataManager).
                // Treat that as a per-setting warning rather than aborting the whole
                // import so the remaining settings still come through.
                try
                {
                    AssetExternal assetExternal = assetsManager.GetExtAsset(globalGameManagersFile, 0, info.PathId);
                    var collection = new ProjectSettingCollection { Assets = { assetExternal } };
                    var outputFilePath = Path.Combine(outputProjectSettingsDirectory, $"{fileName}.{collection.ExportExtension}");
                    SaveCollection(collection, null, outputFilePath, unityVersion);
                    exportedFiles.Add(outputFilePath);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ThunderKit] Skipped importing project setting '{fileName}' ({type}): {e.Message}. " +
                        "This usually means type information for this setting is unavailable in the current class data (tpk).");
                }
            }
            return exportedFiles;
        }

        private void SaveCollection(BaseAssetCollection collection, MetaFile meta, string outputFilePath, UnityVersion unityVersion)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            using (var file = File.Create(outputFilePath))
            using (var streamWriter = new InvariantStreamWriter(file))
            {
                var yamlWriter = new YAMLWriter();
                foreach (var doc in exportManager.Export(collection, assetsManager, unityVersion, exporterInfo))
                {
                    yamlWriter.AddDocument(doc);
                }
                yamlWriter.Write(streamWriter);
            }

            if (meta == null)
            {
                return;
            }

            using (var file = File.Create($"{outputFilePath}.meta"))
            using (var streamWriter = new InvariantStreamWriter(file))
            {
                var yamlWriter = new YAMLWriter
                {
                    IsWriteDefaultTag = false,
                    IsWriteVersion = false
                };
                yamlWriter.AddDocument(meta.ExportYAML());
                yamlWriter.Write(streamWriter);
            }
        }

    }
}
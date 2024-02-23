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
        public bool LogImportErrors;
        private AssetsManager assetsManager;
        private YAMLExportManager exportManager;
        private PPtrExporterInfo pptrExporterInfo;
        private Dictionary<string, object> exporterInfo;
        private string outputProjectSettingsDirectory;

        public override bool Execute()
        {
            if (IncludedSettings == 0) return true;

            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var classDataPath = Path.GetFullPath(Path.Combine(Constants.ThunderKitRoot, "Editor", "ThirdParty", "AssetsTools.NET", "classdata.tpk"));

            var unityVersion = Application.unityVersion;
            var editorDirectory = Path.GetDirectoryName(EditorApplication.applicationPath);
            var executablePath = Path.Combine(settings.GamePath, settings.GameExecutable);
            outputProjectSettingsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "ImportedProjectSettings");

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
            assetsManager.LoadClassDatabaseFromPackage(unityVersion);
            exportManager = YAMLExportManager.CreateDefault();

            var globalGameManagersFile = assetsManager.LoadAssetsFile(Path.Combine(settings.GameDataPath, "globalgamemanagers"), true);


            ExportGlobalGameManagers(globalGameManagersFile, new UnityVersion(unityVersion));

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


        private void ExportGlobalGameManagers(AssetsFileInstance globalGameManagersFile, UnityVersion unityVersion)
        {
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

                try
                {
                    AssetExternal assetExternal = assetsManager.GetExtAsset(globalGameManagersFile, 0, info.PathId);
                    var collection = new ProjectSettingCollection { Assets = { assetExternal } };
                    SaveCollection(collection, null, Path.Combine(outputProjectSettingsDirectory, $"{fileName}.{collection.ExportExtension}"), unityVersion);
                }
                catch (Exception exception)
                {
                    if (LogImportErrors)
                        Debug.LogError($"Caught an error when trying to import external asset from path ID {info.PathId} of the game managers file: {exception}");
                }

            }
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
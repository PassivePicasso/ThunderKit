using AssetsExporter.Collection;
using AssetsExporter.Extensions;
using AssetsExporter.Meta;
using AssetsExporter.YAML;
using AssetsExporter.YAMLExporters.Info;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AssetsExporter
{
    public class GameExporter : IDisposable
    {
        private static readonly Regex FileNameRegex = GenerateFileNameRegex();
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

        private readonly AssetsManager assetsManager;
        private readonly YAMLExportManager exportManager;
        private readonly string outputDirectory;
        private readonly string dataFolder;
        private readonly string outputProjectSettingsDirectory;
        private readonly string outputAssetsPathDirectory;
        private readonly string editorPath;
        private readonly PPtrExporterInfo pptrExporterInfo;
        private readonly Dictionary<string, object> exporterInfo;
        private readonly Dictionary<AssetsFileInstance, Dictionary<long, string>> fileAssetIdToPath;
        private readonly Dictionary<AssetsFileInstance, string> fileToOutputPath;

        private GameExporter(string pathToExecutable, string outputDirectory, string editorPath, string classDataPath)
        {
            pptrExporterInfo = new PPtrExporterInfo();
            exporterInfo = new Dictionary<string, object>
            {
                [nameof(PPtrExporterInfo)] = pptrExporterInfo,
            };
            fileAssetIdToPath = new Dictionary<AssetsFileInstance, Dictionary<long, string>>();
            fileToOutputPath = new Dictionary<AssetsFileInstance, string>();

            exportManager = YAMLExportManager.CreateDefault();
            assetsManager = new AssetsManager();
            assetsManager.LoadClassPackage(classDataPath);

            this.editorPath = editorPath;
            this.outputDirectory = outputDirectory;
            var gameName = Path.GetFileNameWithoutExtension(pathToExecutable);
            dataFolder = Path.Combine(Path.GetDirectoryName(pathToExecutable), $"{gameName}_Data");
            outputProjectSettingsDirectory = Path.Combine(outputDirectory, "ProjectSettings");
            outputAssetsPathDirectory = Path.Combine(outputDirectory, "Assets");
        }

        public static void ExportGame(string pathToExecutable, string outputDirectory, string editorPath, string classDataPath)
        {
            using (var exporter = new GameExporter(pathToExecutable, outputDirectory, editorPath, classDataPath))
            {
                exporter.Export();
            }
        }

        public static void ExportGlobalGameManagers(string pathToExecutable, string outputDirectory, string editorPath, string classDataPath, string unityVersion)
        {
            using (var exporter = new GameExporter(pathToExecutable, outputDirectory, editorPath, classDataPath))
            {
                Directory.CreateDirectory(exporter.outputProjectSettingsDirectory);
                Directory.CreateDirectory(exporter.outputAssetsPathDirectory);

                exporter.ReadEditorExtensionAssemblies();

                var globalGameManagersFile = exporter.assetsManager.LoadAssetsFile(Path.Combine(exporter.dataFolder, "globalgamemanagers"), true);
                exporter.assetsManager.LoadClassDatabaseFromPackage(unityVersion);

                exporter.ExportGlobalGameManagers(globalGameManagersFile, out var buildSettings);
            }
        }

        public void Export()
        {
            Directory.CreateDirectory(outputProjectSettingsDirectory);
            Directory.CreateDirectory(outputAssetsPathDirectory);

            ReadEditorExtensionAssemblies();

            var globalGameManagersFile = assetsManager.LoadAssetsFile(Path.Combine(dataFolder, "globalgamemanagers"), true);
            assetsManager.LoadClassDatabaseFromPackage(globalGameManagersFile.file.typeTree.unityVersion);

            ExportGlobalGameManagers(globalGameManagersFile, out var buildSettings);

            //Preload all asset files and set their outputPath 
            var resourceManager = assetsManager.GetExtAsset(globalGameManagersFile, 0, globalGameManagersFile.table.GetAssetsOfType((int)AssetClassID.ResourceManager).First().index);
            var container = resourceManager.instance.GetBaseField().Get("m_Container").Get("Array");
            var resourceFile = container.childrenCount == 0 ? null : assetsManager.LoadAssetsFile(Path.Combine(dataFolder, "resources.assets"), true);
            if (resourceFile != null)
            {
                var outputResourcesPath = Path.Combine(outputAssetsPathDirectory, "Resources");
                fileToOutputPath[resourceFile] = outputResourcesPath;
            }

            var scenes = buildSettings.instance.GetBaseField().Get("scenes")[0];
            var sharedAssetsFiles = new AssetsFileInstance[scenes.childrenCount];
            var levelFiles = new AssetsFileInstance[scenes.childrenCount];

            for (var i = 0; i < scenes.childrenCount; i++)
            {
                sharedAssetsFiles[i] = assetsManager.LoadAssetsFile(Path.Combine(dataFolder, $"sharedassets{i}.assets"), true);
                fileToOutputPath[sharedAssetsFiles[i]] = outputAssetsPathDirectory;
                //Don't need to set fileToOutputPath for levels, because they don't use ExportAllAssets for export
                levelFiles[i] = assetsManager.LoadAssetsFile(Path.Combine(dataFolder, $"level{i}"), true);
            }

            //Exporting all assets, starting from resources, then each scene sharedAssets => level
            if (resourceFile != null)
            {
                fileAssetIdToPath[resourceFile] = container.children.ToDictionary(
                    el => el.Get("second").GetLastChild().GetValue().AsInt64(),
                    el => el.Get("first").GetValue().AsString());
                ExportAllAssets(resourceFile);
            }

            for (var i = 0; i < scenes.childrenCount; i++)
            {
                ExportAllAssets(sharedAssetsFiles[i]);
            
                var sceneAssetPath = scenes[i].value.value.asString;
                ExportLevel(levelFiles[i], sceneAssetPath);
            }

            CreateAssetsSubDirectoriesMeta();
        }

        private void ReadEditorExtensionAssemblies()
        {
            var extensionsFolder = Path.Combine(editorPath, "Data", "UnityExtensions");
            foreach (var ivyFile in Directory.GetFiles(extensionsFolder, "ivy.xml", SearchOption.AllDirectories))
            {
                var xml = XDocument.Load(ivyFile);
                foreach (var artifact in xml.XPathSelectElements("//artifact"))
                {
                    var assemblyName = Path.GetFileName(artifact.Attribute("name").Value);
                    var ext = artifact.Attribute("ext").Value;
                    var guid = Guid.Parse(artifact.Attribute(XName.Get("guid", "http://ant.apache.org/ivy/extra")).Value);
                    //There can be duplicate names with different guids, for now just taking the last one
                    pptrExporterInfo.unityExtensionAssebmlies[$"{assemblyName}.{ext}"] = guid;
                }
            }
        }

        private void ExportLevel(AssetsFileInstance levelFile, string sceneAssetPath)
        {
            var occlusionSettingsInfo = levelFile.table.GetAssetsOfType((int)AssetClassID.OcclusionCullingSettings).FirstOrDefault();
            Guid guid;
            if (occlusionSettingsInfo != null)
            {
                var occlusionSettings = assetsManager.GetTypeInstance(levelFile, occlusionSettingsInfo);
                var baseField = occlusionSettings.GetBaseField();
                var guidField = baseField.Get("m_SceneGUID");
                guid = new Guid(guidField.children.Select(el => el.value.value.asUInt32).SelectMany(BitConverter.GetBytes).ToArray());
            }
            else
            {
                guid = HashUtils.GetMD5HashGuid(sceneAssetPath);
            }

            var sceneCollection = new SceneCollection();
            sceneCollection.Assets.AddRange(levelFile.table.assetFileInfo.Select(el => assetsManager.GetExtAsset(levelFile, 0, el.index)));
            var sceneMeta = new MetaFile(sceneCollection, guid);

            var outputScenePath = Path.Combine(outputDirectory, sceneAssetPath);

            SaveCollection(sceneCollection, sceneMeta, outputScenePath);

        }

        private void ExportGlobalGameManagers(AssetsFileInstance globalGameManagersFile, out AssetExternal buildSettings)
        {
            exporterInfo["FileIdOverride"] = 1;
            foreach (var info in globalGameManagersFile.table.assetFileInfo)
            {
                if (ignoreTypesOnExport.Contains((AssetClassID)info.curFileType))
                {
                    continue;
                }

                if (!projectSettingAssetToFileName.TryGetValue((AssetClassID)info.curFileType, out var fileName))
                {
                    fileName = Enum.GetName(typeof(AssetClassID), info.curFileType);
                }

                var collection = new AssetCollection { Assets = { assetsManager.GetExtAsset(globalGameManagersFile, 0, info.index) } };
                SaveCollection(collection, null, Path.Combine(outputProjectSettingsDirectory, $"{fileName}.{collection.ExportExtension}"));
            }
            exporterInfo.Remove("FileIdOverride");

            buildSettings = assetsManager.GetExtAsset(globalGameManagersFile, 0, globalGameManagersFile.table.GetAssetsOfType((int)AssetClassID.BuildSettings).First().index);

            var gameUnityVersion = buildSettings.instance.GetBaseField().Get("m_Version").GetValue().value.asString;
            File.WriteAllText(Path.Combine(outputProjectSettingsDirectory, "ProjectVersion.txt"), $"m_EditorVersion: {gameUnityVersion}");
        }

        private void ExportAllAssets(AssetsFileInstance fileInstance)
        {
            var assetToRootAsset = pptrExporterInfo.fileAssetToRootAsset.GetOrAdd(fileInstance);
            foreach (var assetInfo in fileInstance.table.assetFileInfo)
            {
                while (pptrExporterInfo.foundNewCollections.Count > 0)
                {
                    var foundCollection = pptrExporterInfo.foundNewCollections[0];
                    pptrExporterInfo.foundNewCollections.RemoveAt(0);
                    if (ignoreTypesOnExport.Contains((AssetClassID)(foundCollection.MainAsset?.info.curFileType ?? -1u)))
                    {
                        continue;
                    }
                    ExportCollection(foundCollection);
                }

                if (ignoreTypesOnExport.Contains((AssetClassID)assetInfo.curFileType) || assetToRootAsset.ContainsKey(assetInfo.index))
                {
                    continue;
                }

                var asset = assetsManager.GetExtAsset(fileInstance, 0, assetInfo.index);
                var collection = AssetCollection.CreateAssetCollection(assetsManager, asset);

                var mainAssetId = collection.MainAsset.Value.info.index;
                foreach (var cAsset in collection.Assets)
                {
                    assetToRootAsset[cAsset.info.index] = mainAssetId;
                }
                ExportCollection(collection);
            }
        }

        private void ExportCollection(BaseAssetCollection collection)
        {
            var mainAsset = collection.MainAsset.Value;
            var fileInstance = mainAsset.file;
            fileAssetIdToPath.TryGetValue(fileInstance, out var assetIdToPath);
            fileToOutputPath.TryGetValue(fileInstance, out var outputPath);

            var meta = new MetaFile(collection);

            var mainAssetId = mainAsset.info.index;
            if (assetIdToPath != null && assetIdToPath.TryGetValue(mainAssetId, out var assetPath))
            {
                assetPath = Path.Combine(outputPath, $"{assetPath}.{collection.ExportExtension}");
            }
            else
            {
                assetPath = GetUniqueOutputPathForCollection(collection);
            }

            SaveCollection(collection, meta, assetPath);
        }

        private string GetUniqueOutputPathForCollection(BaseAssetCollection collection)
        {
            if (!collection.MainAsset.HasValue)
            {
                throw new ArgumentException("Asset collection must have a main asset");
            }

            var mainAsset = collection.MainAsset.Value;
            var typeString = "";
            if ((AssetClassID)mainAsset.info.curFileType == AssetClassID.MonoBehaviour)
            {
                //if MonoBehaviour is here, then it doesn't have GameObject which means it's a ScriptableObject
                var scriptAsset = assetsManager.GetExtAsset(mainAsset.file, mainAsset.instance.GetBaseField().Get("m_Script"));
                var scriptName = scriptAsset.instance.GetBaseField().Get("m_ClassName").GetValue().AsString();
                typeString = $"ScriptableObject/{scriptName}";
            }
            else if (string.IsNullOrWhiteSpace(typeString))
            {
                typeString = Enum.GetName(typeof(AssetClassID), mainAsset.info.curFileType);
            }

            var outputFolderByType = Path.Combine(outputAssetsPathDirectory, typeString);
            var name = FileNameRegex.Replace(TryGetAssetName(mainAsset), "_");

            var assetPath = Path.Combine(outputFolderByType, $"{name}.{collection.ExportExtension}");
            if (!File.Exists(assetPath))
            {
                return assetPath;
            }

            for (var i = 1; i < int.MaxValue; i++)
            {
                assetPath = Path.Combine(outputFolderByType, $"{name} {i}.{collection.ExportExtension}");
                if (!File.Exists(assetPath))
                {
                    return assetPath;
                }
            }
            return name;
        }

        private string TryGetAssetName(AssetExternal asset)
        {
            var baseField = asset.instance.GetBaseField();
            var nameField = baseField.Get("m_Name");
            var name = "";

            if (!nameField.IsDummy())
            {
                name = nameField.GetValue().AsString();
            }

            if (string.IsNullOrWhiteSpace(name) && (AssetClassID)asset.info.curFileType == AssetClassID.Shader)
            {
                name = baseField.Get("m_ParsedForm").Get("m_Name").GetValue().AsString();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Unnamed";
            }

            return name;
        }

        private void SaveCollection(BaseAssetCollection collection, MetaFile meta, string outputFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            using (var file = File.Create(outputFilePath))
            using (var streamWriter = new InvariantStreamWriter(file))
            {
                var yamlWriter = new YAMLWriter();
                foreach (var doc in exportManager.Export(collection, assetsManager, exporterInfo))
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

        private void CreateAssetsSubDirectoriesMeta()
        {
            if (!Directory.Exists(outputAssetsPathDirectory))
            {
                return;
            }
            foreach (var dir in Directory.GetDirectories(outputAssetsPathDirectory, "*", SearchOption.AllDirectories))
            {
                var metaPath = dir + ".meta";
                if (File.Exists(metaPath))
                {
                    continue;
                }

                var relativeDirPath = dir.Substring(outputDirectory.Length + 1);
                var meta = new MetaFile(relativeDirPath);
                using (var file = File.Create(metaPath))
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

        private static Regex GenerateFileNameRegex()
        {
            var invalidChars = new string(Path.GetInvalidFileNameChars());
            var escapedChars = Regex.Escape(invalidChars);
            return new Regex($"[{escapedChars}]");
        }

        public void Dispose()
        {
            assetsManager.UnloadAll(true);
        }
    }
}

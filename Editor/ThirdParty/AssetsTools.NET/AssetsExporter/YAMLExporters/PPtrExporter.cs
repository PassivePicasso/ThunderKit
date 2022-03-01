using AssetsExporter.Collection;
using AssetsExporter.Extensions;
using AssetsExporter.YAML;
using AssetsExporter.YAMLExporters.Info;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsExporter.YAMLExporters
{
    public class PPtrExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            var node = new YAMLMappingNode(MappingStyle.Flow);
            var fileID = field.Get("m_FileID").GetValue().value.asInt32;
            var pathID = field.Get("m_PathID").GetValue().value.asInt64;

            if (pathID == 0)
            {
                node.Add("fileID", 0);
                return node;
            }

            if (fileID != 0)
            {
                var dep = context.SourceAsset.file.file.dependencies.dependencies[fileID - 1];
                if (dep.guid != Guid.Empty)
                {
                    node.Add("fileID", pathID);
                    node.Add("guid", dep.guid.ToString("N"));
                    node.Add("type", 0);
                    return node;
                }
            }

            var file = fileID == 0 ? context.SourceAsset.file : context.SourceAsset.file.GetDependency(context.AssetsManager, fileID - 1);
            var info = context.Info.GetOrAdd<PPtrExporterInfo>(nameof(PPtrExporterInfo));

            var depInfo = context.AssetsManager.GetExtAsset(file, 0, pathID, true);
            if ((AssetClassID)depInfo.info.curFileType == AssetClassID.MonoScript)
            {
                var scriptsCache = info.scriptsCache.GetOrAdd(file);
                if (!scriptsCache.TryGetValue(pathID, out var scriptRef))
                {
                    var scriptAsset = context.AssetsManager.GetExtAsset(file, 0, pathID);
                    var scriptBase = scriptAsset.instance.GetBaseField();

                    var className = scriptBase.Get("m_ClassName").GetValue().value.asString;
                    var @namespace = scriptBase.Get("m_Namespace").GetValue().value.asString;
                    var assemblyName = scriptBase.Get("m_AssemblyName").GetValue().value.asString;

                    var scriptFileID = HashUtils.ComputeScriptFileID(@namespace, className);
                    //Unity has guids for their extension assemblies in editor folder (ivy.xml files), use them if found
                    if (!info.unityExtensionAssebmlies.TryGetValue(assemblyName, out var scriptGuid))
                    {
                        scriptGuid = HashUtils.GetMD5HashGuid(Path.GetFileNameWithoutExtension(assemblyName));
                    }
                    scriptsCache[pathID] = scriptRef = new KeyValuePair<long, Guid>(scriptFileID, scriptGuid);
                }

                node.Add("fileID", scriptRef.Key);
                node.Add("guid", scriptRef.Value.ToString("N"));
                node.Add("type", 3);
                return node;
            }
            
            if (fileID == 0 && context.Collection.Assets.Any(el => el.info.index == pathID))
            {
                node.Add("fileID", pathID);
                return node;
            }

            var assetToRootAsset = info.fileAssetToRootAsset.GetOrAdd(file);
            if (!assetToRootAsset.TryGetValue(pathID, out var rootPathID))
            {
                var dependencyCollection = AssetCollection.CreateAssetCollection(context.AssetsManager, context.AssetsManager.GetExtAsset(file, 0, pathID));
                info.foundNewCollections.Add(dependencyCollection);
                rootPathID = dependencyCollection.MainAsset.Value.info.index;
                foreach (var cAsset in dependencyCollection.Assets)
                {
                    assetToRootAsset[cAsset.info.index] = rootPathID;
                }
            }

            node.Add("fileID", pathID);
            node.Add("guid", HashUtils.GetMD5HashGuid($"{rootPathID}_{file.name}").ToString("N"));
            node.Add("type", 2);
            return node;
        }
    }
}

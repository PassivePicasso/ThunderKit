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
            var fileID = field.Get("m_FileID").AsInt;
            var pathID = field.Get("m_PathID").AsLong;

            if (pathID == 0)
            {
                node.Add("fileID", 0);
                return node;
            }

            if (fileID != 0)
            {
                var dep = context.SourceAsset.file.file.Metadata.Externals[fileID - 1];
                if (!dep.Guid.IsEmpty)
                {
                    node.Add("fileID", pathID);
                    node.Add("guid", dep.Guid.ToString());
                    node.Add("type", 0);
                    return node;
                }
            }

            var file = fileID == 0 ? context.SourceAsset.file : context.SourceAsset.file.GetDependency(context.AssetsManager, fileID - 1);
            var info = context.Info.GetOrAdd<PPtrExporterInfo>(nameof(PPtrExporterInfo));

            var depInfo = context.AssetsManager.GetExtAsset(file, 0, pathID, true);
            if ((AssetClassID)depInfo.info.TypeId == AssetClassID.MonoScript)
            {
                var scriptsCache = info.scriptsCache.GetOrAdd(file);
                if (!scriptsCache.TryGetValue(pathID, out var scriptRef))
                {
                    var scriptAsset = context.AssetsManager.GetExtAsset(file, 0, pathID);
                    var scriptBase = scriptAsset.baseField;

                    var className = scriptBase.Get("m_ClassName").AsString;
                    var @namespace = scriptBase.Get("m_Namespace").AsString;
                    var assemblyName = scriptBase.Get("m_AssemblyName").AsString;

                    var fullName = $"{assemblyName}, {@namespace}{(string.IsNullOrEmpty(@namespace) ? "" : ".")}{className}";
                    if (!info.typeReferenceOverrides.TryGetValue(fullName, out scriptRef))
                    {
                        scriptsCache[pathID] = scriptRef;
                    }
                    else
                    {
                        var scriptFileID = HashUtils.ComputeScriptFileID(@namespace, className);
                        //Unity has guids for their extension assemblies in editor folder (ivy.xml files), use them if found
                        if (!info.unityExtensionAssebmlies.TryGetValue(assemblyName, out var scriptGuid))
                        {
                            scriptGuid = HashUtils.GetMD5HashGuid(Path.GetFileNameWithoutExtension(assemblyName));
                        }
                        scriptsCache[pathID] = scriptRef = (scriptFileID, scriptGuid, 3);
                    }
                }

                node.Add("fileID", scriptRef.pathID);
                node.Add("guid", scriptRef.fileID.ToString("N"));
                node.Add("type", scriptRef.type);
                return node;
            }
            
            if (fileID == 0 && context.Collection.Assets.Any(el => el.info.PathId == pathID))
            {
                node.Add("fileID", pathID);
                return node;
            }

            var assetToRootAsset = info.fileAssetToRootAsset.GetOrAdd(file);
            if (!assetToRootAsset.TryGetValue(pathID, out var rootPathID))
            {
                var dependencyCollection = AssetCollection.CreateAssetCollection(context.AssetsManager, context.AssetsManager.GetExtAsset(file, 0, pathID));
                if (info.storeFoundCollections)
                {
                    info.foundNewCollections.Add(dependencyCollection);
                }
                rootPathID = dependencyCollection.MainAsset.Value.info.PathId;
                foreach (var cAsset in dependencyCollection.Assets)
                {
                    assetToRootAsset[cAsset.info.PathId] = rootPathID;
                }
            }

            node.Add("fileID", pathID);
            node.Add("guid", HashUtils.GetMD5HashGuid($"{rootPathID}_{file.name}").ToString("N"));
            node.Add("type", 2);
            return node;
        }
    }
}

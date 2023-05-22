using AssetsExporter.Collection;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.YAMLExporters.Info
{
    public class PPtrExporterInfo
    {
        public readonly Dictionary<AssetsFileInstance, Dictionary<long, long>> fileAssetToRootAsset = new Dictionary<AssetsFileInstance, Dictionary<long, long>>();
        public readonly Dictionary<AssetsFileInstance, Dictionary<long, (long pathID, Guid fileID, int type)>> scriptsCache = new Dictionary<AssetsFileInstance, Dictionary<long, (long, Guid, int)>>();
        public readonly Dictionary<string, Guid> unityExtensionAssebmlies = new Dictionary<string, Guid>();
        public readonly Dictionary<string, (long pathID, Guid fileID, int type)> typeReferenceOverrides = new Dictionary<string, (long, Guid, int)>();
        public readonly List<BaseAssetCollection> foundNewCollections = new List<BaseAssetCollection>();
        public bool storeFoundCollections;
    }
}

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
        public readonly Dictionary<AssetsFileInstance, Dictionary<long, KeyValuePair<long, Guid>>> scriptsCache = new Dictionary<AssetsFileInstance, Dictionary<long, KeyValuePair<long, Guid>>>();
        public readonly Dictionary<string, Guid> unityExtensionAssebmlies = new Dictionary<string, Guid>();
        public readonly List<BaseAssetCollection> foundNewCollections = new List<BaseAssetCollection>();
    }
}

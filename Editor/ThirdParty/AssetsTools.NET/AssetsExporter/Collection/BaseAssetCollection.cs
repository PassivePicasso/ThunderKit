using AssetsExporter.Meta;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.Collection
{
    public abstract class BaseAssetCollection
    {
        public List<AssetExternal> Assets { get; } = new List<AssetExternal>();
        public virtual AssetExternal? MainAsset => Assets.Count == 0 ? null : Assets[0] as AssetExternal?;
        public virtual string ExportExtension => "asset";
        public virtual Type ImporterType => typeof(NativeFormatImporter);
        public virtual (string, string) GetTagAndAnchor(AssetExternal asset)
        {
            return (asset.info.TypeId.ToString(), asset.info.PathId.ToString());
        }
    }
}

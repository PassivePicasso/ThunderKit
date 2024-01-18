using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.Collection
{
    public class ProjectSettingCollection : BaseAssetCollection
    {
        public override (string, string) GetTagAndAnchor(AssetExternal asset)
        {
            return (asset.info.TypeId.ToString(), "1");
        }
    }
}

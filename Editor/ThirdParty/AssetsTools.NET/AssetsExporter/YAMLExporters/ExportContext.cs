using AssetsExporter.Collection;
using AssetsExporter.YAML;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.YAMLExporters
{
    public class ExportContext
    {
        public YAMLExportManager ExportManager { get; }
        public AssetsManager AssetsManager { get; }
        public AssetExternal SourceAsset { get; }
        public BaseAssetCollection Collection { get; }
        public Dictionary<string, object> Info { get; }

        public ExportContext(YAMLExportManager exportManager, AssetsManager assetsManager, BaseAssetCollection collection, AssetExternal sourceAsset, Dictionary<string, object> info = null)
        {
            ExportManager = exportManager;
            AssetsManager = assetsManager;
            Collection = collection;
            SourceAsset = sourceAsset;
            Info = info ?? new Dictionary<string, object>();
        }

        public YAMLNode Export(AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false, Type ignoreExporterType = null)
        {
            return ExportManager.Export(this, parentField, field, raw, ignoreExporterType);
        }
    }
}

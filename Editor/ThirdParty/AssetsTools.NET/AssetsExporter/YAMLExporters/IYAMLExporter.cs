using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;
using AssetsExporter.YAML;

namespace AssetsExporter.YAMLExporters
{
    public interface IYAMLExporter
    {
        YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false);
    }
}

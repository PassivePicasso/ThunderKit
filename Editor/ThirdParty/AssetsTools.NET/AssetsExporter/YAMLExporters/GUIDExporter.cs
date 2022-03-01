using AssetsExporter.YAML;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsExporter.YAMLExporters
{
    public class GUIDExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            return new YAMLScalarNode(new Hash128(field.children.Select(el => el.GetValue().AsUInt())).ToGuid().ToString("N"));
        }
    }
}

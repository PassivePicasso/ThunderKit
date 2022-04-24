using AssetsExporter.YAML;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.YAMLExporters
{
    public class ComponentPairExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            var node = new YAMLMappingNode();
            var pair = field.children[0];
            node.Add(pair.GetName(), context.Export(field, pair, raw));

            return node;
        }
    }
}

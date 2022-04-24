using AssetsExporter.YAML;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.YAMLExporters
{
    public class PairExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            var node = new YAMLMappingNode();
            var key = context.Export(field, field.children[0]);
            var value = context.Export(field, field.children[1]);
            if (field.children[0].templateField.hasValue)
            {
                node.Add(key, value);
            }
            else
            {
                node.Add(field.children[0].GetName(), key);
                node.Add(field.children[1].GetName(), value);
            }
            return node;
        }
    }
}

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
            var key = context.Export(field, field.Children[0]);
            var value = context.Export(field, field.Children[1]);
            if (field.Children[0].TemplateField.HasValue)
            {
                node.Add(key, value);
            }
            else
            {
                node.Add(field.Children[0].FieldName, key);
                node.Add(field.Children[1].FieldName, value);
            }
            return node;
        }
    }
}

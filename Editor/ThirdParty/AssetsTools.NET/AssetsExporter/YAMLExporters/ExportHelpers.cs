using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;
using AssetsExporter.YAML;

namespace AssetsExporter.YAMLExporters
{
    internal class ExportHelpers
    {
        public static YAMLNode ExportArray(ExportContext context, AssetTypeValueField arrayField)
        {
            switch (arrayField.TemplateField.ValueType)
            {
                case AssetValueType.ByteArray:
                    return arrayField.AsByteArray.ExportYAML();
                case AssetValueType.Array:
                    var node = new YAMLSequenceNode();
                    if (arrayField.Children.Count > 0)
                    {
                        for (var i = 0; i < arrayField.Children.Count; i++)
                        {
                            node.Add(context.Export(arrayField, arrayField.Children[i]));
                        }
                    }

                    return node;
                default:
                    throw new NotSupportedException("Not supported field type");
            }
        }
    }
}

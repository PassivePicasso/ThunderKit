using AssetsExporter.YAML;
using AssetsExporter.YAML.Utils.Extensions;
using AssetsTools.NET;
using System;

namespace AssetsExporter.YAMLExporters
{
    public class GenericExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            switch (field.TemplateField.ValueType)
            {
                case AssetValueType.Array:
                    return ExportHelpers.ExportArray(context, field);
                case AssetValueType.ByteArray:
                    return field.AsByteArray.ExportYAML();
                case AssetValueType.None:
                    break;
                default:
                    throw new NotSupportedException("Value types are not supported for this exporter");
            }

            if (field.Children.Count == 1 && field.Children[0].TemplateField.IsArray)
            {
                var arrayChild = field.Children[0];
                if (field.TemplateField.Type == "map" && arrayChild.TemplateField.Children[1].Children[0].HasValue)
                {
                    var node = new YAMLMappingNode();
                    for (var i = 0; i < arrayChild.Children.Count; i++)
                    {
                        var elem = arrayChild.Children[i];
                        node.Add(context.Export(arrayChild, elem.Children[0]), context.Export(arrayChild, elem.Children[1]));
                    }
                    return node;
                }
                return ExportHelpers.ExportArray(context, arrayChild);
            }

            if (field.Children.Count > 0)
            {
                var node = new YAMLMappingNode();
                node.AddSerializedVersion(field.TemplateField.Version);
                for (var i = 0; i < field.Children.Count; i++)
                {
                    var child = field.Children[i];
                    node.Add(child.TemplateField.Name, context.Export(field, child));
                }
                return node;
            }

            return new YAMLMappingNode();
        }
    }
}

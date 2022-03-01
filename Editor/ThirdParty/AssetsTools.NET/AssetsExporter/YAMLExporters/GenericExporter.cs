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
            switch (field.templateField.valueType)
            {
                case EnumValueTypes.Array:
                    return ExportHelpers.ExportArray(context, field);
                case EnumValueTypes.ByteArray:
                    return field.GetValue().value.asByteArray.data.ExportYAML();
                case EnumValueTypes.None:
                    break;
                default:
                    throw new NotSupportedException("Value types are not supported for this exporter");
            }

            if (field.childrenCount == 1 && field.children[0].templateField.isArray)
            {
                var arrayChild = field.children[0];
                if (field.templateField.type == "map" && arrayChild.templateField.children[1].children[0].hasValue)
                {
                    var node = new YAMLMappingNode();
                    for (var i = 0; i < arrayChild.childrenCount; i++)
                    {
                        var elem = arrayChild.children[i];
                        node.Add(context.Export(arrayChild, elem.children[0]), context.Export(arrayChild, elem.children[1]));
                    }
                    return node;
                }
                return ExportHelpers.ExportArray(context, arrayChild);
            }

            if (field.childrenCount > 0)
            {
                var node = new YAMLMappingNode();
                node.AddSerializedVersion(field.templateField.version);
                for (var i = 0; i < field.childrenCount; i++)
                {
                    var child = field.children[i];
                    node.Add(child.templateField.name, context.Export(field, child));
                }
                return node;
            }

            return new YAMLMappingNode();
        }
    }
}

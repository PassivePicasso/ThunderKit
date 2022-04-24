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
            switch (arrayField.templateField.valueType)
            {
                case EnumValueTypes.ByteArray:
                    return arrayField.GetValue().value.asByteArray.data.ExportYAML();
                case EnumValueTypes.Array:
                    var node = new YAMLSequenceNode();
                    if (arrayField.childrenCount > 0)
                    {
                        for (var i = 0; i < arrayField.childrenCount; i++)
                        {
                            node.Add(context.Export(arrayField, arrayField.children[i]));
                        }
                    }

                    return node;
                default:
                    throw new NotSupportedException("Not supported field type");
            }
        }
    }
}

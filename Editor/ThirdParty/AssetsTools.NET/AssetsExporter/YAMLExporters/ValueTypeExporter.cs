using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;
using AssetsExporter.YAML;

namespace AssetsExporter.YAMLExporters
{
    public class ValueTypeExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            var value = field.GetValue()?.value ?? default;
            switch (field.templateField.valueType)
            {
                case EnumValueTypes.Bool:
                    return new YAMLScalarNode(value.asBool);
                case EnumValueTypes.Int8:
                    return new YAMLScalarNode(value.asInt8);
                case EnumValueTypes.Int16:
                    return new YAMLScalarNode(value.asInt16);
                case EnumValueTypes.Int32:
                    return new YAMLScalarNode(value.asInt32);
                case EnumValueTypes.Int64:
                    return new YAMLScalarNode(value.asInt64);
                case EnumValueTypes.UInt8:
                    return new YAMLScalarNode(value.asUInt8, raw);
                case EnumValueTypes.UInt16:
                    return new YAMLScalarNode(value.asUInt16, raw);
                case EnumValueTypes.UInt32:
                    return new YAMLScalarNode(value.asUInt32, raw);
                case EnumValueTypes.UInt64:
                    return new YAMLScalarNode(value.asUInt64, raw);
                case EnumValueTypes.Float:
                    return new YAMLScalarNode(value.asFloat);
                case EnumValueTypes.Double:
                    return new YAMLScalarNode(value.asDouble);
                case EnumValueTypes.String:
                    return new YAMLScalarNode(value.asString);
            }

            throw new NotSupportedException("Complex types are not supported for this exporter");
        }
    }
}

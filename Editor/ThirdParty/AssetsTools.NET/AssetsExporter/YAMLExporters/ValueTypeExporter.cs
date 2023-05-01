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
            var value = field.Value ?? default;
            switch (field.TemplateField.ValueType)
            {
                case AssetValueType.Bool:
                    return new YAMLScalarNode(value.AsBool);
                case AssetValueType.Int8:
                    return new YAMLScalarNode(value.AsSByte);
                case AssetValueType.Int16:
                    return new YAMLScalarNode(value.AsShort);
                case AssetValueType.Int32:
                    return new YAMLScalarNode(value.AsInt);
                case AssetValueType.Int64:
                    return new YAMLScalarNode(value.AsLong);
                case AssetValueType.UInt8:
                    return new YAMLScalarNode(value.AsByte, raw);
                case AssetValueType.UInt16:
                    return new YAMLScalarNode(value.AsUShort, raw);
                case AssetValueType.UInt32:
                    return new YAMLScalarNode(value.AsUInt, raw);
                case AssetValueType.UInt64:
                    return new YAMLScalarNode(value.AsULong, raw);
                case AssetValueType.Float:
                    return new YAMLScalarNode(value.AsFloat);
                case AssetValueType.Double:
                    return new YAMLScalarNode(value.AsDouble);
                case AssetValueType.String:
                    return new YAMLScalarNode(value.AsString);
            }

            throw new NotSupportedException("Complex types are not supported for this exporter");
        }
    }
}

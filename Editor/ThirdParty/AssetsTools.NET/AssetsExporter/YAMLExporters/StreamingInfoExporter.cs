using AssetsExporter.YAML;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.YAMLExporters
{
    /// <summary>
    /// Exporting empty StreamingInfo because it will be handled in TypelessDataExporter
    /// </summary>
    public class StreamingInfoExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            return context.Export(parentField, ValueBuilder.DefaultValueFieldFromTemplate(field.templateField), false, typeof(StreamingInfoExporter));
        }
    }
}

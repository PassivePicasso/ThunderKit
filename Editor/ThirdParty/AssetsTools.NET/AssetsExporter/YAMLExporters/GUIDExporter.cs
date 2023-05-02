using AssetsExporter.YAML;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsExporter.YAMLExporters
{
    public class GUIDExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            var guid = new GUID128
            {
                data0 = field.Children[0].AsUInt,
                data1 = field.Children[1].AsUInt,
                data2 = field.Children[2].AsUInt,
                data3 = field.Children[3].AsUInt
            };
            return new YAMLScalarNode(guid.ToString());
        }
    }
}

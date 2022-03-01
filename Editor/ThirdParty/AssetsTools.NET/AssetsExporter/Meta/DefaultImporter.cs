using AssetsExporter.Collection;
using AssetsExporter.YAML;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.Meta
{
    public class DefaultImporter : BaseImporter
    {
        public override string Name => nameof(DefaultImporter);

        public override YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add("externalObjects", new YAMLMappingNode());
            node.Add("userData", new YAMLScalarNode());
            node.Add("assetBundleName", new YAMLScalarNode());
            node.Add("assetBundleVariant", new YAMLScalarNode());
            return node;
        }
    }
}

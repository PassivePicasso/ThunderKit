using AssetsExporter.YAML;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.YAMLExporters
{
    /// <summary>
    /// Supposedly that should stop Unity from sometimes messing up shader assets
    /// </summary>
    public class ShaderEmptyDependenciesExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            var node = new YAMLMappingNode();

            foreach (var child in field.children)
            {
                if (child.GetName() == "m_Dependencies")
                {
                    node.Add(child.GetName(), new YAMLSequenceNode());
                    continue;
                }
                node.Add(child.GetName(), context.Export(field, child));
            }

            return node;
        }
    }
}

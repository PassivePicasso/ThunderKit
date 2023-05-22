using AssetsExporter.YAML;
using AssetsExporter.YAML.Utils.Extensions;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsExporter.YAMLExporters
{
    public class GraphicsSettingsExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            var node = new YAMLMappingNode();
            node.AddSerializedVersion(field.TemplateField.Version);

            var tierSettings = new List<AssetTypeValueField>();
            foreach (var cField in field.Children)
            {
                if (cField.TypeName == "TierGraphicsSettings")
                {
                    tierSettings.Add(cField);
                    continue;
                }

                node.Add(cField.FieldName, context.Export(field, cField, raw, typeof(GraphicsSettingsExporter)));
            }

            if (tierSettings.Count > 0)
            {
                var tiersNode = new YAMLSequenceNode();

                var currentTier = 0;
                foreach (var tier in tierSettings)
                {
                    var tierNode = new YAMLMappingNode();
                    tierNode.Add("m_BuildTarget", 1);
                    tierNode.Add("m_Tier", currentTier++);
                    tierNode.Add("m_Settings", context.Export(field, tier, raw, typeof(GraphicsSettingsExporter)));
                    tierNode.Add("m_Automatic", false);
                }

                node.Add("m_TierSettings", tiersNode);
            }

            return node;
        }
    }
}

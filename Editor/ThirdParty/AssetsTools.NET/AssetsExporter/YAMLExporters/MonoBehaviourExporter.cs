using AssetsExporter.YAML;
using AssetsExporter.YAML.Utils.Extensions;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsExporter.YAMLExporters
{
    public class MonoBehaviourExporter : IYAMLExporter
    {
        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false)
        {
            var baseField = field;
            if (!context.SourceAsset.file.file.typeTree.hasTypeTree)
            {
                var managedPath = Path.Combine(Path.GetDirectoryName(context.SourceAsset.file.path), "Managed");
                if (!Directory.Exists(managedPath))
                {
                    throw new DirectoryNotFoundException($"Asset file doesn't have a TypeTree and couldn't find \"Managed\" folder at {managedPath}");
                }
                baseField = context.AssetsManager.GetMonoBaseFieldCached(context.SourceAsset.file, context.SourceAsset.info, managedPath) ?? field;
            }

            return context.Export(parentField, baseField, false, typeof(MonoBehaviourExporter));
        }
    }
}

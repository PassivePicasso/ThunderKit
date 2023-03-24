using AssetsExporter.Collection;
using AssetsExporter.YAML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsExporter.Meta
{
    public class NativeFormatImporter : DefaultImporter
    {
        public override string Name => nameof(NativeFormatImporter);

        public long mainObjectFileID;
        public override void AssignCollection(BaseAssetCollection collection)
        {
            mainObjectFileID = collection.Assets.First().info.PathId;
        }

        public override YAMLNode ExportYAML()
        {
            var node = base.ExportYAML() as YAMLMappingNode;
            node.Add("mainObjectFileID", mainObjectFileID);
            return node;
        }
    }
}

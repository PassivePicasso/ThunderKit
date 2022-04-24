using AssetsExporter.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.Collection
{
    public class SceneCollection : AssetCollection
    {
        public override Type ImporterType => typeof(DefaultImporter);
        public override string ExportExtension => "unity";
    }
}

using AssetsExporter.Collection;
using AssetsExporter.YAML;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsExporter.Meta
{
    public abstract class BaseImporter
    {
        public abstract string Name { get; } 
        public virtual void AssignCollection(BaseAssetCollection collection) { }
        public abstract YAMLNode ExportYAML();
    }
}

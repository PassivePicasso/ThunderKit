using AssetsExporter.Collection;
using AssetsExporter.YAML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsExporter.Meta
{
    public class MetaFile
    {
        public int FileFormatVersion { get; } = 2;
        public Guid Guid { get; private set; }
        public BaseImporter Importer { get; private set; }
        public bool FolderAsset { get; private set; }

        public MetaFile(string relativeFolderPath)
        {
            FolderAsset = true;
            //Assuming that relative path starts from project root and was made using Directory or Path classes so it has correct separator for current platform
            //Then replacing that separator with '/', so that hash would be independent of a platform
            Guid = HashUtils.GetMD5HashGuid(relativeFolderPath.Replace(Path.DirectorySeparatorChar, '/'));
            Importer = new DefaultImporter();
        }

        public MetaFile(BaseAssetCollection collection) : this(collection, CreateCollectionGuid(collection)) { }

        public MetaFile(BaseAssetCollection collection, Guid guid)
        {
            FolderAsset = false;
            Guid = guid;
            Importer = Activator.CreateInstance(collection.ImporterType) as BaseImporter;
            Importer.AssignCollection(collection);
        }

        public YAMLDocument ExportYAML()
        {
            var doc = new YAMLDocument();
            var root = doc.CreateMappingRoot();

            root.Add("fileFormatVersion", FileFormatVersion);
            root.Add("guid", Guid.ToString("N"));
            if (FolderAsset)
            {
                root.Add("folderAsset", true);
            }
            root.Add(Importer.Name, Importer.ExportYAML());
            return doc;
        }

        private static Guid CreateCollectionGuid(BaseAssetCollection collection)
        {
            var mainAsset = collection.MainAsset;
            if (!mainAsset.HasValue)
            {
                return Guid.Empty;
            }
            return HashUtils.GetMD5HashGuid($"{mainAsset.Value.info.PathId}_{mainAsset.Value.file.name}");
        }
    }
}

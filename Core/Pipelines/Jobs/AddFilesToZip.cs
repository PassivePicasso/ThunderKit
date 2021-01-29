#if CompressionInstalled
using System.IO;
using System.IO.Compression;
using UnityEditor;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class AddFilesToZip : PipelineJob
    {
        public CompressionLevel Compression;
        public DefaultAsset TargetZipFile;
        public DefaultAsset[] Files;
        public override void Execute(Pipeline pipeline)
        {
            using (var fileStream = File.Open(AssetDatabase.GetAssetPath(TargetZipFile), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Update))
                foreach (var file in Files)
                {
                    var filePath = AssetDatabase.GetAssetPath(file);
                    archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath), Compression);
                }
        }
    }
}
#endif
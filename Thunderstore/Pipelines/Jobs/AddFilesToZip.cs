#if CompressionInstalled
using UnityEditor;
using System.IO.Compression;
using System.IO;

namespace PassivePicasso.ThunderKit.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class AddFilesToZip : PipelineJob
    {
        public CompressionLevel Compression;
        public DefaultAsset TargetZipFile;
        public DefaultAsset[] Files;
        public override void Execute(Pipeline pipeline)
        {
            using (var fileStream = File.OpenRead(AssetDatabase.GetAssetPath(TargetZipFile)))
            using (var archive = new ZipArchive(fileStream))
                foreach (var file in Files)
                {
                    var filePath = AssetDatabase.GetAssetPath(file);
                    archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath), Compression);
                }
        }
    }
}
#endif
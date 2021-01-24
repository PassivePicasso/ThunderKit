#if CompressionInstalled
using ThunderKit.Core.Pipelines;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor]
    public class ZipStaging : PipelineJob
    {
        public bool ShowOutput;
        public override void Execute(Pipeline pipeline)
        {
            //var outputRoot = Path.Combine("ThunderKit", "Staging", nameof(ZipStaging), pipeline.Manifest.name);
            //var outputFile = Path.Combine(outputRoot, $"{pipeline.Manifest.name}.zip");
            //var outputMetaFile = Path.Combine(outputRoot, $"{pipeline.Manifest.name}.zip.meta");

            //if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
            //Directory.CreateDirectory(outputRoot);

            //ZipFile.CreateFromDirectory(pipeline.ManifestPath, outputFile);

            //if (ShowOutput)
            //    Process.Start(outputRoot);
        }
    }
}
#endif
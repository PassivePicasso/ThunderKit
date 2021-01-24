#if CompressionInstalled
#if UNITY_EDITOR
using ThunderKit.Core.Pipelines;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class ZipStaging : PipelineJob
    {
        public bool ShowOutput;
        public override void Execute(Pipeline pipeline)
        {
            var manifestPipeline = pipeline as ManifestPipeline;
            var outputRoot = Path.Combine("ThunderKit", "Staging", nameof(ZipStaging), manifestPipeline.Manifest.name);
            var outputFile = Path.Combine(outputRoot, $"{manifestPipeline.Manifest.name}.zip");
            var outputMetaFile = Path.Combine(outputRoot, $"{manifestPipeline.Manifest.name}.zip.meta");

            if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
            Directory.CreateDirectory(outputRoot);

            ZipFile.CreateFromDirectory(manifestPipeline.ManifestPath, outputFile);

            if (ShowOutput)
                Process.Start(outputRoot);
        }
    }
}
#endif
#endif
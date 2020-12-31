#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using System.IO;
using System.IO.Compression;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class ZipStaging : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            var manifestPipeline = pipeline as ManifestPipeline;

            string outputFile = Path.Combine(pipeline.OutputRoot, $"{manifestPipeline.Manifest.name}.zip");
            if (File.Exists(outputFile)) File.Delete(outputFile);

            ZipFile.CreateFromDirectory(manifestPipeline.ManifestPath, outputFile);
        }
    }
}
#endif
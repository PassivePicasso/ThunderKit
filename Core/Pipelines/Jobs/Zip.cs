using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jops
{
    [PipelineSupport(typeof(Pipeline))]
    public class Zip : PipelineJob
    {
        public bool PerManifest;

        [PathReferenceResolver]
        public string SourcePath;
        [PathReferenceResolver]
        public string OutputFile;
        public bool ShowOutput;

        public override void Execute(Pipeline pipeline)
        {
            if (PerManifest)
            {
                for (pipeline.ManifestIndex = 0; pipeline.ManifestIndex < pipeline.manifests.Length; pipeline.ManifestIndex++)
                    ExecuteInternal(pipeline);
                pipeline.ManifestIndex = -1;
            }
            else
                ExecuteInternal(pipeline);
        }

        private void ExecuteInternal(Pipeline pipeline)
        {
            var outputFile = OutputFile.Resolve(pipeline, this);
            var outputRoot = Path.GetDirectoryName(outputFile);

            if (File.Exists(outputFile)) File.Delete(outputFile);
            Directory.CreateDirectory(outputRoot);

            ZipFile.CreateFromDirectory(SourcePath.Resolve(pipeline, this), outputFile);

            if (ShowOutput)
                Process.Start(outputRoot);
        }
    }
}
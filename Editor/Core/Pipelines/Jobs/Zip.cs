using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jops
{
    [PipelineSupport(typeof(Pipeline))]
    public class Zip : FlowPipelineJob
    {
        [PathReferenceResolver]
        public string SourcePath;
        [PathReferenceResolver]
        public string OutputFile;
        public bool ShowOutput;

        protected override void ExecuteInternal(Pipeline pipeline)
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
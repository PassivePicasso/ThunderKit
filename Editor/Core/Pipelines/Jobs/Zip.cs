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
        public CompressionLevel Compression;
        public bool IncludeBaseDirectory;
        [PathReferenceResolver]
        public string Source;
        [PathReferenceResolver]
        public string Output;

        protected override void ExecuteInternal(Pipeline pipeline)
        {
            var output = Output.Resolve(pipeline, this);
            var source = Source.Resolve(pipeline, this);
            var outputDir = Path.GetDirectoryName(output);

            File.Delete(output);

            Directory.CreateDirectory(outputDir);

            ZipFile.CreateFromDirectory(source, output, Compression, IncludeBaseDirectory);
        }
    }
}
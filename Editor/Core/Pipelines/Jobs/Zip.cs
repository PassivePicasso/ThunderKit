using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jops
{
    [PipelineSupport(typeof(Pipeline))]
    public class Zip : FlowPipelineJob
    {
        public ArchiveType ArchiveType = ArchiveType.Zip;

        [PathReferenceResolver]
        public string Source;
        [PathReferenceResolver]
        public string Output;

        protected override Task ExecuteInternal(Pipeline pipeline)
        {
            var output = Output.Resolve(pipeline, this);
            var source = Source.Resolve(pipeline, this);
            var outputDir = Path.GetDirectoryName(output);

            if (File.Exists(output))
            {
                File.Delete(output);
            }

            Directory.CreateDirectory(outputDir);

            using (var archive = ArchiveFactory.Create(ArchiveType))
            {
                archive.AddAllFromDirectory(source, searchOption: SearchOption.AllDirectories);
                var options = new WriterOptions(CompressionType.Deflate);
                archive.SaveTo(output, options);
            }

            int i = 1;
            var copiedFiles = Directory.EnumerateFiles(outputDir, "*", SearchOption.AllDirectories)
                .Prepend("Copied Files")
                .Aggregate((a, b) => $"{a}\r\n\r\n {i++}. {b}");
            pipeline.Log(LogLevel.Information, $"archived ``` {source} ``` into ``` {output} ```", copiedFiles);

            return Task.CompletedTask;
        }
    }
}

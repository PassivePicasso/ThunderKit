using System.IO;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class CopyFile : PipelineJob
    {
        [PathReferenceResolver]
        public string SourceFile;
        [PathReferenceResolver]
        public string DestinationFile;

        public override void Execute(Pipeline pipeline)
        {
            var sourceFile = SourceFile.Resolve(pipeline, this);
            var destinationFile = DestinationFile.Resolve(pipeline, this);

            File.Copy(sourceFile, destinationFile, true);
        }
    }
}
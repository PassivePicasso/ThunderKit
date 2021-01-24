using System.IO;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class CopyFile : PipelineJob
    {
        public string SourceFile;
        public string DestinationFile;

        public override void Execute(Pipeline pipeline)
        {
            var sourceFile = PathReference.ResolvePath(SourceFile, pipeline);
            var destinationFile = PathReference.ResolvePath(DestinationFile, pipeline);

            File.Copy(sourceFile, destinationFile, true);
        }
    }
}
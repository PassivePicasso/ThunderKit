using System.IO;
using PassivePicasso.ThunderKit.Core.Pipelines;
using UnityEditor;

namespace ThunderKit.Core.Pipelines.Composable.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class CopyFile : PipelineJob
    {
        public string SourceFile;
        public string DestinationFile;

        public override void Execute(Pipeline pipeline)
        {
            var sourceFile = PathReference.ResolvePath(SourceFile, null, pipeline);
            var destinationFile = PathReference.ResolvePath(DestinationFile, null, pipeline);

            File.Copy(sourceFile, destinationFile, true);
        }
    }
}
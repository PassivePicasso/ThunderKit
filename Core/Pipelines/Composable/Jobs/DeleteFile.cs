using System.IO;

namespace ThunderKit.Core.Pipelines.Composable.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class DeleteFile : PipelineJob
    {
        public string FilePath;

        public override void Execute(Pipeline pipeline)
        {
            var file = PathReference.ResolvePath(FilePath, null, pipeline);

            if (File.Exists(file)) File.Delete(file);
        }
    }
}

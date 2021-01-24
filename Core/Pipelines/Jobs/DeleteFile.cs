using System.IO;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class DeleteFile : PipelineJob
    {
        public string FilePath;

        public override void Execute(Pipeline pipeline)
        {
            var file = PathReference.ResolvePath(FilePath, pipeline);

            if (File.Exists(file)) File.Delete(file);
        }
    }
}

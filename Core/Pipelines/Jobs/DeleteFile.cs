using System.IO;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Attributes;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), SingleLine]
    public class DeleteFile : PipelineJob
    {
        public string deleteFile;

        public override void Execute(Pipeline pipeline)
        {
            var file = deleteFile.Resolve(pipeline, this);

            if (File.Exists(file)) File.Delete(file);
        }
    }
}

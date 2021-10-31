using System.IO;
using ThunderKit.Core.Paths;
using ThunderKit.Core;
using System;
using System.Threading.Tasks;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class Delete : FlowPipelineJob
    {
        public bool Recursive;
        public bool IsFatal;
        public string Path;

        protected override Task ExecuteInternal(Pipeline pipeline)
        {
            var path = Path.Resolve(pipeline, this);
            var pathIsFile = false;
            try
            {
                pathIsFile = !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch (Exception e)
            {
                if (IsFatal)
                    throw e;
            }

            try
            {
                pipeline.Log(LogLevel.Information, $"Deleting {path}");
                if (pathIsFile) File.Delete(path);
                else
                    Directory.Delete(path, Recursive);
                pipeline.Log(LogLevel.Information, $"Deleted {path}");
            }
            catch
            {
                if (IsFatal)
                    throw;
            }

            return Task.CompletedTask;
        }
    }
}

using ThunderKit.Core.Attributes;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class ExecutePipeline : PipelineJob
    {
        public bool InheritRootManifest;
        public Pipeline executePipeline;
        public override void Execute(Pipeline pipeline)
        {
            if (!executePipeline) return;

            if (InheritRootManifest && pipeline.manifest)
            {
                var manifest = executePipeline.manifest;
                executePipeline.manifest = pipeline.manifest;
                executePipeline.Execute();
                executePipeline.manifest = manifest;
            }
            else
                executePipeline.Execute();
        }
    }
}

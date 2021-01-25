using ThunderKit.Core.Attributes;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), SingleLine]
    public class ExecutePipeline : PipelineJob
    {
        public Pipeline executePipeline;
        public override void Execute(Pipeline pipeline)
        {
            if (!executePipeline) return;

            if (pipeline.manifests)
            {
                var currManifests = executePipeline.manifests;
                executePipeline.manifests = pipeline.manifests;
                executePipeline.Execute();
                executePipeline.manifests = currManifests;
            }
            else
                executePipeline.Execute();
        }
    }
}

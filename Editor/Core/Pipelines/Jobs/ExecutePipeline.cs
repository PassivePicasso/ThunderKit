using System.Threading.Tasks;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class ExecutePipeline : PipelineJob
    {
        public bool OverrideManifest;
        public Pipeline targetpipeline;
        public override async Task Execute(Pipeline pipeline)
        {
            if (!targetpipeline) return;

            // pipeline.manifest is the correct field to use, stop checking every time.
            // pipieline.manifest is the manifest that is assigned to the pipeline containing this job via the editor
            var manifest = targetpipeline.manifest;
            PipelineLog priorLogger = null;
            try
            {
                priorLogger = targetpipeline.Logger;
                targetpipeline.Logger = pipeline.Logger;
                if (OverrideManifest && pipeline.manifest)
                {
                    targetpipeline.manifest = pipeline.manifest;
                    await targetpipeline.Execute();
                }
                else
                    await targetpipeline.Execute();
            }
            finally
            {
                targetpipeline.manifest = manifest;
                targetpipeline.Logger = priorLogger;
            }
        }
    }
}

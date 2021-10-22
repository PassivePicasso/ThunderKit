using System.Threading.Tasks;
using ThunderKit.Core.Attributes;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class ExecutePipeline : PipelineJob
    {
        public bool OverrideManifest;
        public Pipeline executePipeline;
        public override async Task Execute(Pipeline pipeline)
        {
            void ExecutePipeline_LogUpdated(object sender, LogEntry e) => pipeline.Log(e);
            if (!executePipeline) return;

            // pipeline.manifest is the correct field to use, stop checking every time.
            // pipieline.manifest is the manifest that is assigned to the pipeline via the editor
            try
            {
                executePipeline.LogUpdated += ExecutePipeline_LogUpdated;
                if (OverrideManifest && pipeline.manifest)
                {
                    var manifest = executePipeline.manifest;
                    executePipeline.manifest = pipeline.manifest;
                    await executePipeline.Execute();
                    executePipeline.manifest = manifest;

                }
                else
                    await executePipeline.Execute();
            }
            finally
            {
                executePipeline.LogUpdated -= ExecutePipeline_LogUpdated;
            }
        }
    }
}

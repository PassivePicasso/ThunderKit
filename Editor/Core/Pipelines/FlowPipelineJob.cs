using System.Linq;
using System.Threading.Tasks;

namespace ThunderKit.Core.Pipelines
{
    public abstract class FlowPipelineJob : PipelineJob
    {
        public bool PerManifest;
        public Manifests.Manifest[] ExcludedManifests;

        public sealed override async Task Execute(Pipeline pipeline)
        {
            if (PerManifest)
            {
                for (pipeline.ManifestIndex = 0;
                     pipeline.ManifestIndex < pipeline.Manifests.Length;
                     pipeline.ManifestIndex++)
                {
                    if (ExcludedManifests.Contains(pipeline.Manifest)) continue;

                    await ExecuteInternal(pipeline);
                }
                pipeline.ManifestIndex = -1;
            }
            else
                await ExecuteInternal(pipeline);
        }

        protected abstract Task ExecuteInternal(Pipeline pipeline);
    }
}
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Serialization;

namespace ThunderKit.Core.Pipelines
{
    public abstract class FlowPipelineJob : PipelineJob
    {
        public enum ManifestListType { BlackList, WhiteList }

        public bool PerManifest;
        public ManifestListType ListType;
        [FormerlySerializedAs("ExcludedManifests")]
        public Manifests.Manifest[] Manifests;

        public sealed override async Task Execute(Pipeline pipeline)
        {
            if (PerManifest)
            {
                for (pipeline.ManifestIndex = 0;
                     pipeline.ManifestIndex < pipeline.Manifests.Length;
                     pipeline.ManifestIndex++)
                {
                    switch (ListType)
                    {
                        case ManifestListType.BlackList:
                            if (Manifests.Contains(pipeline.Manifest)) continue;
                            break;
                        case ManifestListType.WhiteList:
                            if (!Manifests.Contains(pipeline.Manifest)) continue;
                            break;
                    }

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

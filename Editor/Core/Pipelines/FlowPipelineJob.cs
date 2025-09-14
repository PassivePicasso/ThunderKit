using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Common.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThunderKit.Core.Pipelines
{
    public abstract class FlowPipelineJob : PipelineJob
    {
        public enum ManifestListType { [InspectorName("Deny List")] BlackList, [InspectorName("Allow List")] WhiteList }

        public bool PerManifest;
        public ManifestListType ListType;
        [FormerlySerializedAs("ExcludedManifests")]
        public Manifests.Manifest[] Manifests;

        public sealed override async Task Execute(Pipeline pipeline)
        {
            if (PerManifest)
            {
                var processedCount = 0;
                var totalManifests = pipeline.Manifests?.Length ?? 0;

                for (pipeline.ManifestIndex = 0;
                     pipeline.ManifestIndex < pipeline.Manifests.Length;
                     pipeline.ManifestIndex++)
                {
                    var manifest = pipeline.Manifest;
                    var shouldProcess = true;

                    switch (ListType)
                    {
                        case ManifestListType.BlackList:
                            if (Manifests.Contains(manifest))
                            {
                                shouldProcess = false;
                                pipeline.Log(LogLevel.Information,
                                    $"Manifest '{manifest?.name}' excluded by Deny List");
                            }
                            break;
                        case ManifestListType.WhiteList:
                            if (!Manifests.Contains(manifest))
                            {
                                shouldProcess = false;
                                var allowlistNames = string.Join(", ", Manifests.Select(m => m?.name ?? "null"));
                                pipeline.Log(LogLevel.Information,
                                    $"Manifest '{manifest?.name}' excluded by Allow List. Allowed manifests: [{allowlistNames}]");
                            }
                            break;
                    }

                    if (shouldProcess)
                    {
                        processedCount++;
                        await ExecuteInternal(pipeline);
                    }
                }

                if (processedCount == 0 && totalManifests > 0)
                {
                    var listTypeName = ListType == ManifestListType.BlackList ? "Deny List" : "Allow List";
                    var manifestNames = string.Join(", ", Manifests.Select(m => m?.name ?? "null"));
                    pipeline.Log(LogLevel.Warning,
                        $"No manifests processed due to {listTypeName} filtering. " +
                        $"Available manifests: {totalManifests}, {listTypeName}: [{manifestNames}]. " +
                        $"Check ListType and Manifests configuration.");
                }

                pipeline.ManifestIndex = -1;
            }
            else
                await ExecuteInternal(pipeline);
        }

        protected abstract Task ExecuteInternal(Pipeline pipeline);
    }
}

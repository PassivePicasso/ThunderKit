#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Data;
using PassivePicasso.ThunderKit.Pipelines;
using System.Linq;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class StageUnityPackages : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            var manifest = (pipeline as ManifestPipeline).Manifest;
            if (manifest.unityPackages?.Any() != true) return;

            foreach (var redistributable in manifest.unityPackages)
                UnityPackage.Export(redistributable, pipeline.OutputRoot);
        }
    }
}
#endif
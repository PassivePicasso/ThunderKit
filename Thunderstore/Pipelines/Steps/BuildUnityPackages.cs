#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;
using System.IO;
using System.Linq;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class BuildUnityPackages : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            var output/**/ = Path.Combine(pipeline.OutputRoot, pipeline.name);
            var manifest = (pipeline as ManifestPipeline).Manifest;
            if (manifest.unityPackages?.Any() != true) return;

            foreach (var redistributable in manifest.unityPackages)
                UnityPackage.Export(redistributable, output);
        }
    }
}
#endif
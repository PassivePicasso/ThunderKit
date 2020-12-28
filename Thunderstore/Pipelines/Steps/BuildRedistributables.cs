#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline))]
    public class BuildRedistributables : PipelineJob
    {
        public string RedistributablesOutputPath;

        public override void Execute(Pipeline pipeline)
        {
            var manifest = (pipeline as ManifestPipeline).Manifest;

            foreach (var redistributable in manifest.redistributables)
            {
                UnityPackage.Export(redistributable, RedistributablesOutputPath);
            }
        }
    }
}
#endif
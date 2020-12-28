#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;
using System.IO;
using UnityEditor;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline))]
    public class BuildAssetBundles : PipelineJob
    {
        [EnumFlag]
        public BuildAssetBundleOptions AssetBundleBuildOptions;
        public BuildTarget buildTarget;
        public bool OverrideOutputPath;
        public string AssetBundlePath;

        public override void Execute(Pipeline pipeline)
        {
            var manifest = (pipeline as ManifestPipeline).Manifest;
            var output/*   */= Path.Combine(pipeline.OutputRoot, pipeline.name);
            var bepinexDir/*     */= Path.Combine(output, "BepInExPack", "BepInEx");

            var targetPath = OverrideOutputPath ? Path.Combine(AssetBundlePath, "plugins", manifest.name)
                                                : Path.Combine(bepinexDir, "plugins", manifest.name);

            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

            BuildPipeline.BuildAssetBundles(targetPath, AssetBundleBuildOptions, buildTarget);
        }
    }
}
#endif
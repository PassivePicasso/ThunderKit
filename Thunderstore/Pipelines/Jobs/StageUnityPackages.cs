#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Data;
using PassivePicasso.ThunderKit.Pipelines;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class StageUnityPackages : PipelineJob
    {
        [Tooltip("Output path Relative to the Manifest Staging root. \r\nUse %Manifest to add the Manifest name")]
        public string outputPath;
        public override void Execute(Pipeline pipeline)
        {
            var manifestPipeline = pipeline as ManifestPipeline;
            var manifest = manifestPipeline.Manifest;
            if (manifest.unityPackages?.Any() != true) return;

            string resolvedOutputPath = outputPath.Replace("%Manifest/", $"{manifest.name}/").Replace("%Manifest\\", $"{manifest.name}\\");
            var outputRoot = Path.Combine(manifestPipeline.ManifestPath, resolvedOutputPath);
            
            if (!Directory.Exists(outputRoot)) Directory.CreateDirectory(outputRoot);

            foreach (var redistributable in manifest.unityPackages)
                UnityPackage.Export(redistributable, outputRoot);
        }
    }
}
#endif
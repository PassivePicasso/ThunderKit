#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class StageManifestFiles : PipelineJob
    {
        public bool includeReadme;
        public bool includeIcon;
        public bool includeManifestJson;

        public override void Execute(Pipeline pipeline)
        {
            var manifestPipeline = pipeline as ManifestPipeline;
            var manifest = manifestPipeline.Manifest;
            var outputPath = Path.Combine(manifestPipeline.ManifestPath);

            if (includeReadme)
                if (manifest.readme)
                {
                    var readmePath = AssetDatabase.GetAssetPath(manifest.readme);
                    File.Copy(readmePath, Path.Combine(outputPath, "README.md"), true);
                }
                else File.WriteAllText(Path.Combine(outputPath, "README.md"), $"# {manifest.name}");

            if (includeManifestJson)
            {
                var manifestJson = manifest.RenderJson();
                var pluginPath = Path.Combine(outputPath, "plugins", manifest.name);
                if (manifest.plugins.Any() && !Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);

                if (Directory.Exists(pluginPath)) File.WriteAllText(Path.Combine(pluginPath, "manifest.json"), manifestJson);

                File.WriteAllText(Path.Combine(outputPath, "manifest.json"), manifestJson);
            }

            if (includeIcon && manifest.icon)
                File.WriteAllBytes(Path.Combine(outputPath, "icon.png"), manifest.icon.EncodeToPNG());
        }
    }
}
#endif
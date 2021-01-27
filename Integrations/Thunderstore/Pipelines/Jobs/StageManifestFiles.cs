using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;
using ThunderKit.Integrations.Thunderstore.Manifests;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor, RequiresManifestDatumType(typeof(ThunderstoreManifest))]
    public class StageManifestFiles : PipelineJob
    {
        public bool includeReadme;
        public bool includeIcon;
        public bool includeManifestJson;

        public override void Execute(Pipeline pipeline)
        {
            foreach (var manifest in pipeline.Manifest.Data.OfType<ThunderstoreManifest>())
            {
                var manifestJson = includeManifestJson ? string.Empty : RenderJson(manifest);

                foreach (var outputPath in manifest.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                {
                    if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

                    if (includeReadme)
                        if (manifest.readme)
                        {
                            var readmePath = AssetDatabase.GetAssetPath(manifest.readme);
                            File.Copy(readmePath, Path.Combine(outputPath, "README.md"), true);
                        }
                        else File.WriteAllText(Path.Combine(outputPath, "README.md"), $"# {manifest.name}");

                    if (includeManifestJson)
                    {
                        manifestJson = RenderJson(manifest);
                        var pluginPath = Path.Combine(outputPath, "plugins", manifest.name);

                        if (Directory.Exists(pluginPath)) File.WriteAllText(Path.Combine(pluginPath, "manifest.json"), manifestJson);

                        File.WriteAllText(Path.Combine(outputPath, "manifest.json"), manifestJson);
                    }

                    if (includeIcon && manifest.icon)
                        File.WriteAllBytes(Path.Combine(outputPath, "icon.png"), manifest.icon.EncodeToPNG());
                }
            }
        }

        public string RenderJson(ThunderstoreManifest manifest)
        {
            var manifestJson = JsonUtility.ToJson(manifest);

            manifestJson = manifestJson.Substring(1);
            manifestJson = $"{{\"name\":\"{name}\",{manifestJson}";
            return manifestJson;
        }
    }
}
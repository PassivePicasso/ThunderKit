using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;
using ThunderKit.Integrations.Thunderstore.Manifests;
using UnityEditor;
using UnityEngine;
using static ThunderKit.Integrations.Thunderstore.CreateThunderstoreManifest;

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
                var manifestJson = includeManifestJson ? string.Empty : RenderJson(manifest, pipeline.Manifest.name);

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
                        manifestJson = RenderJson(manifest, pipeline.Manifest.name);
                        var pluginPath = Path.Combine(outputPath, "plugins", manifest.name);

                        if (Directory.Exists(pluginPath)) File.WriteAllText(Path.Combine(pluginPath, "manifest.json"), manifestJson);

                        File.WriteAllText(Path.Combine(outputPath, "manifest.json"), manifestJson);
                    }

                    if (includeIcon && manifest.icon)
                        File.WriteAllBytes(Path.Combine(outputPath, "icon.png"), manifest.icon.EncodeToPNG());
                }
            }
        }

        public string RenderJson(ThunderstoreManifest manifest, string name) => 
            JsonUtility.ToJson(new ThunderstoreManifestStub
            {
                author = manifest.author,
                dependencies = manifest.dependencies.ToArray(),
                description = manifest.description,
                name = name,
                version_number = manifest.versionNumber,
                website_url = manifest.url
            });
    }
}
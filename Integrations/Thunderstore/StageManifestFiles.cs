using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Manifests.Datum;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;
using static ThunderKit.Integrations.Thunderstore.CreateThunderstoreManifest;

namespace ThunderKit.Integrations.Thunderstore.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor, RequiresManifestDatumType(typeof(ThunderstoreData), typeof(ManifestIdentity))]
    public class StageManifestFiles : PipelineJob
    {
        public bool includeReadme;
        public bool includeIcon;
        public bool includeManifestJson;

        public override void Execute(Pipeline pipeline)
        {
            var thunderstoreData = pipeline.Manifest.Data.OfType<ThunderstoreData>().First();
            var identity = pipeline.Manifest.Data.OfType<ManifestIdentity>().First();
            var manifestJson = includeManifestJson ? RenderJson(identity, thunderstoreData, pipeline.Manifest.name) : string.Empty;

            foreach (var outputPath in thunderstoreData.StagingPaths.Select(path => path.Resolve(pipeline, this)))
            {
                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

                if (includeReadme)
                    if (thunderstoreData.readme)
                    {
                        var readmePath = AssetDatabase.GetAssetPath(thunderstoreData.readme);
                        File.Copy(readmePath, Path.Combine(outputPath, "README.md"), true);
                    }
                    else File.WriteAllText(Path.Combine(outputPath, "README.md"), $"# {identity.Name}");

                if (includeManifestJson)
                {
                    var pluginPath = Path.Combine(outputPath, "plugins", identity.Name);
                    File.WriteAllText(Path.Combine(outputPath, "manifest.json"), manifestJson);
                }

                if (includeIcon && thunderstoreData.icon)
                    File.WriteAllBytes(Path.Combine(outputPath, "icon.png"), thunderstoreData.icon.EncodeToPNG());
            }
        }

        public string RenderJson(ManifestIdentity identity, ThunderstoreData manifest, string name)
        {
            var dependencies = identity.Dependencies.Select(man => {
                var id = man.Data.OfType<ManifestIdentity>().First();
                return $"{id.Author}-{id.Name}-{id.Version}";
            });
            var stub = new ThunderstoreManifestStub
            {
                author = identity.Author,
                dependencies = dependencies.ToArray(),
                description = identity.Description,
                name = identity.Name,
                version_number = identity.Version,
                website_url = manifest.url
            };
            return JsonUtility.ToJson(stub);
        }
    }
}
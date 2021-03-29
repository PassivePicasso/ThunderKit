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
    using static ThunderKit.Common.PathExtensions;

    [PipelineSupport(typeof(Pipeline)), ManifestProcessor, RequiresManifestDatumType(typeof(ThunderstoreData), typeof(ManifestIdentity))]
    public class StageThunderstoreManifest : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            var thunderstoreData = pipeline.Manifest.Data.OfType<ThunderstoreData>().First();
            var identity = pipeline.Manifest.Data.OfType<ManifestIdentity>().First();
            var manifestJson = RenderJson(identity, thunderstoreData);

            foreach (var outputPath in thunderstoreData.StagingPaths.Select(path => path.Resolve(pipeline, this)))
            {
                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

                File.WriteAllText(Combine(outputPath, "manifest.json"), manifestJson);
            }
        }

        public string RenderJson(ManifestIdentity identity, ThunderstoreData manifest)
        {
            var dependencies = identity.Dependencies.Select(man =>
            {
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
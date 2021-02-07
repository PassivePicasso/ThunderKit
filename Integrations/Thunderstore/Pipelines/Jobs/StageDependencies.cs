using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;
using ThunderKit.Integrations.Thunderstore.Manifests;

namespace ThunderKit.Integrations.Thunderstore.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(ThunderstoreManifest))]
    public class StageDependencies : PipelineJob
    {
        [PathReferenceResolver]
        public string StagingPath;
        public override void Execute(Pipeline pipeline)
        {
            var thunderstoreManifests = pipeline.Datums.OfType<ThunderstoreManifest>();
            var dependencies = thunderstoreManifests.SelectMany(tm => tm.dependencies).Distinct().ToList();

            foreach (var manifest in pipeline.manifests)
                foreach (var tm in manifest.Data.OfType<ThunderstoreManifest>())
                    dependencies.RemoveAll(dep => dep.StartsWith($"{tm.author}-{manifest.name}"));

            var packages = Path.Combine("Packages");
            var dependencyPaths = dependencies.Select(dep => Path.Combine(packages, dep));

            foreach(var dependencyPath in dependencyPaths)
                CopyFilesRecursively(dependencyPath, StagingPath.Resolve(pipeline, this));
        }

        public static void CopyFilesRecursively(string source, string destination)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace("Packages", destination));

            foreach (string filePath in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(filePath).Equals(".meta")) continue;

                string destFileName = filePath.Replace("Packages", destination);
                Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
                File.Copy(filePath, destFileName, true);
            }
        }
    }
}

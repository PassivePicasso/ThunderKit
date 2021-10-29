using System.IO;
using ThunderKit.Core.Attributes;
using System.Threading.Tasks;
using ThunderKit.Core.Paths;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class StageDependencies : PipelineJob
    {
        [PathReferenceResolver]
        public string StagingPath;
        public Manifests.Manifest[] ExcludedManifests;
        public override Task Execute(Pipeline pipeline)
        {
            for (pipeline.ManifestIndex = 0; pipeline.ManifestIndex < pipeline.Manifests.Length; pipeline.ManifestIndex++)
            {
                if (ArrayUtility.Contains(ExcludedManifests, pipeline.Manifest)) continue;
                if (AssetDatabase.GetAssetPath(pipeline.Manifest).StartsWith("Assets")) continue;

                var manifestIdentity = pipeline.Manifest.Identity;
                var dependencyPath = Path.Combine("Packages", manifestIdentity.Name);
                string destination = StagingPath.Resolve(pipeline, this);
                var manifestPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(pipeline.Manifest));

                var results = CopyFilesRecursively(dependencyPath, destination).Prepend("Staged Files").ToArray();
                pipeline.Log(LogLevel.Information, $"Staged Dependency [{manifestIdentity.Name}](assetlink://{manifestPath })", results);
            }

            pipeline.ManifestIndex = -1;
            return Task.CompletedTask;
        }

        public static IEnumerable<string> CopyFilesRecursively(string source, string destination)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace("Packages", destination));

            foreach (string filePath in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(filePath).Equals(".meta")) continue;

                string destFileName = filePath.Replace("Packages", destination);
                Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
                File.Copy(filePath, destFileName, true);
                yield return $"From {filePath}\r\n\r\n  To {destFileName}";
            }
        }

    }
}

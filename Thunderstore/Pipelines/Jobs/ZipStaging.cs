#if CompressionInstalled
#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Data;
using PassivePicasso.ThunderKit.Pipelines;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class ZipStaging : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            var manifestPipeline = pipeline as ManifestPipeline;
            var packageJsonPath = Path.Combine("Packages", nameof(ZipStaging), "package.json");
            var outputRoot = Path.Combine("Packages", nameof(ZipStaging), manifestPipeline.Manifest.name);
            var outputFile = Path.Combine(outputRoot, $"{manifestPipeline.Manifest.name}.zip");

            if (File.Exists(outputFile)) File.Delete(outputFile);
            if (!Directory.Exists(outputRoot)) Directory.CreateDirectory(outputRoot);

            ZipFile.CreateFromDirectory(manifestPipeline.ManifestPath, outputFile);

            if (!File.Exists(packageJsonPath))
            {
                File.WriteAllText(packageJsonPath, JsonUtility.ToJson(new PackageManagerManifest(
                      "thunderkit.zipped",
                      $"{nameof(ZipStaging)} Output",
                      "1.0.0",
                      Application.unityVersion.Substring(0, Application.unityVersion.LastIndexOf(".")),
                      $"Output from {nameof(ZipStaging)}"
                  )));

            }

            AssetDatabase.Refresh();
        }
    }
}
#endif
#endif
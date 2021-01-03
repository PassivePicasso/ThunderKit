#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Data;
using PassivePicasso.ThunderKit.Pipelines;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class StageUnityPackages : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            var manifest = (pipeline as ManifestPipeline).Manifest;
            if (manifest.unityPackages?.Any() != true) return;
            var packageJsonPath = Path.Combine("Packages", nameof(StageUnityPackages), "package.json");
            
            var outputRoot = Path.Combine("Packages", nameof(StageUnityPackages), manifest.name);
            if (!Directory.Exists(outputRoot)) Directory.CreateDirectory(outputRoot);

            if (!File.Exists(packageJsonPath))
            {
                File.WriteAllText(packageJsonPath, JsonUtility.ToJson(new PackageManagerManifest(
                      "thunderkit.unitypackages",
                      $"{nameof(StageUnityPackages)} Output",
                      "1.0.0",
                      Application.unityVersion.Substring(0, Application.unityVersion.LastIndexOf(".")),
                      $"Output from {nameof(StageUnityPackages)}"
                  )));

            }
            foreach (var redistributable in manifest.unityPackages)
                UnityPackage.Export(redistributable, outputRoot);

            AssetDatabase.Refresh();
        }
    }
}
#endif
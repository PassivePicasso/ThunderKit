#if CompressionInstalled
using PassivePicasso.ThunderKit.Core.Data;
using PassivePicasso.ThunderKit.Core.Pipelines;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class ZipFiles : PipelineJob
    {
        public System.IO.Compression.CompressionLevel Compression;
        public string OutputName;
        public DefaultAsset[] Files;

        public override void Execute(Pipeline pipeline)
        {
            var packageFolder = Path.Combine("Packages", nameof(ZipFiles));
            var packageJsonPath = Path.Combine(packageFolder, "package.json");
            var zipfilePath = Path.Combine(packageFolder, $"{OutputName}.zip");

            if (!File.Exists(packageJsonPath))
            {
                File.WriteAllText(packageJsonPath, JsonUtility.ToJson(new PackageManagerManifest(
                      "thunderkit.zipped",
                      $"{nameof(ZipFiles)} Output",
                      "1.0.0",
                      Application.unityVersion.Substring(0, Application.unityVersion.LastIndexOf(".")),
                      $"Output from {nameof(ZipFiles)}"
                  )));
            }

            using (var fileStream = File.Create(zipfilePath))
            using (var archive = new ZipArchive(fileStream))
                foreach (var file in Files)
                {
                    var filePath = AssetDatabase.GetAssetPath(file);
                    archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath), Compression);
                }

            AssetDatabase.Refresh();
        }
    }
}
#endif
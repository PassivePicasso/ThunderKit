using System.IO;
using System.IO.Compression;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore
{
    public class PackageLoader
    {
        private static string Packages = Path.Combine("Packages");
        private const string ROS_Temp = "ros_temp";

        static FileSystemWatcher PackagesWatcher;
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            if (PackagesWatcher == null)
            {
                PackagesWatcher = new FileSystemWatcher(ROS_Temp, "*.zip");

                PackagesWatcher.Created += PackagesWatcher_Created;
                PackagesWatcher.Changed += PackagesWatcher_Created;
                PackagesWatcher.EnableRaisingEvents = true;
            }
        }

        private static void PackagesWatcher_Created(object sender, FileSystemEventArgs e)
        {
            string filePath = e.FullPath;
            EditorApplication.update += InstallPackage;

            void InstallPackage()
            {
                try
                {
                    EditorApplication.update -= InstallPackage;

                    AssetDatabase.StartAssetEditing();

                    string fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
                    var dependencyPath = Path.Combine(Packages, fileNameNoExt);
                    if (Directory.Exists(dependencyPath)) Directory.Delete(dependencyPath, true);
                    if (File.Exists($"{dependencyPath}.meta")) File.Delete($"{dependencyPath}.meta");

                    Directory.CreateDirectory(dependencyPath);

                    using (var fileStream = File.OpenRead(filePath))
                    using (var archive = new ZipArchive(fileStream))
                        foreach (var entry in archive.Entries)
                        {
                            if (entry.FullName.ToLower().EndsWith("/") || entry.FullName.ToLower().EndsWith("\\"))
                                continue;

                            var outputPath = Path.Combine(dependencyPath, entry.FullName);
                            var outputDir = Path.GetDirectoryName(outputPath);
                            var fileName = Path.GetFileName(outputPath);

                            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

                            entry.ExtractToFile(outputPath);
                            if ("manifest.json".Equals(fileName.ToLower()))
                            {
                                var stubManifest = CreateThunderstoreManifest.LoadStub(outputPath);
                                string name = stubManifest.name.ToLower();
                                string modVersion = stubManifest.version_number;
                                string description = stubManifest.description;

                                string unityVersion = Application.unityVersion.Substring(0, Application.unityVersion.LastIndexOf("."));
                                var authorAlias = fileNameNoExt.Substring(0, fileNameNoExt.IndexOf('-'));
                                var author = new Author
                                {
                                    name = authorAlias,
                                    url = stubManifest.website_url
                                };
                                var packageManifest = new PackageManagerManifest(author, name, ObjectNames.NicifyVariableName(stubManifest.name), modVersion, unityVersion, description);
                                var packageManifestJson = JsonUtility.ToJson(packageManifest);

                                File.WriteAllText(Path.Combine(outputDir, "package.json"), packageManifestJson);
                            }
                        }
                    File.Delete(filePath);
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
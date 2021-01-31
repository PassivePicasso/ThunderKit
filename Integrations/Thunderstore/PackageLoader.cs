using System.IO;
using System.IO.Compression;
using ThunderKit.Core.Data;
using ThunderKit.Core.Editor;
using UnityEditor;
using UnityEngine;
namespace ThunderKit.Integrations.Thunderstore
{
    using static Constants;
    public class PackageLoader
    {

        static FileSystemWatcher PackagesWatcher;
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            if (PackagesWatcher == null)
            {
                Directory.CreateDirectory(TempDir);
                PackagesWatcher = new FileSystemWatcher(TempDir, "*.zip");

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
                                var authorAlias = fileNameNoExt.Substring(0, fileNameNoExt.IndexOf('-'));
                                GeneratePackageManifest(stubManifest, authorAlias, stubManifest.name.ToLower(), outputDir);
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

        public static void GeneratePackageManifest(CreateThunderstoreManifest.ThunderstoreManifestStub stubManifest, string authorAlias, string name, string outputDir)
        {
            string modVersion = stubManifest.version_number;
            string description = stubManifest.description;

            string unityVersion = Application.unityVersion.Substring(0, Application.unityVersion.LastIndexOf("."));
            var author = new Author
            {
                name = authorAlias,
                url = stubManifest.website_url
            };
            var packageManifest = new PackageManagerManifest(author, name, ObjectNames.NicifyVariableName(stubManifest.name), modVersion, unityVersion, description);
            var packageManifestJson = JsonUtility.ToJson(packageManifest);
            ScriptingSymbolManager.AddScriptingDefine(name);
            File.WriteAllText(Path.Combine(outputDir, "package.json"), packageManifestJson);
        }
    }
}
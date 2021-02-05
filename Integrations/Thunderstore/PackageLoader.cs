using System.IO;
using System.IO.Compression;
using ThunderKit.Common.Package;
using UnityEditor;

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
                            if(Path.GetExtension(fileName).Equals(".dll"))
                            {
                                string assemblyPath = outputPath;
                                PackageHelper.WriteAssemblyMetaData(assemblyPath, $"{assemblyPath}.meta");
                            }
                            if ("manifest.json".Equals(fileName.ToLower()))
                            {
                                var stubManifest = CreateThunderstoreManifest.LoadStub(outputPath);
                                var authorAlias = fileNameNoExt.Substring(0, fileNameNoExt.IndexOf('-'));
                                PackageHelper.GeneratePackageManifest(
                                    stubManifest.name.ToLower(), outputDir,
                                    stubManifest.name, authorAlias,
                                    stubManifest.version_number,
                                    stubManifest.description,
                                    stubManifest.website_url);
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
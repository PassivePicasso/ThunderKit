using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Common.Package;
using ThunderKit.Core.Editor;
using ThunderKit.Core.Data;
using UnityEditor;

namespace ThunderKit.Integrations.Thunderstore
{
    using PV = Core.Data.PackageVersion;
    public class ThunderstoreSource : PackageSource
    {
        [InitializeOnLoadMethod]
        public static async void Initialize()
        {
            await ThunderstoreAPI.ReloadPages();
            var assetPath = $"Assets/ThunderKitSettings/{typeof(ThunderstoreSource).Name}.asset";

            var isNew = false;
            var source = ScriptableHelper.EnsureAsset<ThunderstoreSource>(assetPath, so =>
            {
                isNew = true;
            });
            if (isNew)
            {
                source.LoadPackages();
                source.hideFlags = UnityEngine.HideFlags.NotEditable;
                EditorUtility.SetDirty(source);
                AssetDatabase.SaveAssets();
            }
        }

        public override string Name => "Thunderstore";

        public override string SourceGroup => "Thunderstore";

        protected override void LoadPackagesInternal()
        {
            var loadedPackages = ThunderstoreAPI.LookupPackage(string.Empty).ToArray();
            var activePackages = loadedPackages.Where(tsp => !tsp.is_deprecated).ToArray();
            var realMods = activePackages.Where(tsp => !tsp.categories.Contains("Modpacks")).ToArray();
            var orderByPinThenName = realMods.OrderByDescending(tsp => tsp.is_pinned).ThenBy(tsp => tsp.name).ToArray();
            foreach (var tsp in orderByPinThenName)
            {
                AddPackageGroup(tsp.owner, tsp.name, tsp.latest.description, tsp.full_name, tsp.categories, () =>
                {
                    return tsp.versions.Select(v => (v.version_number, v.full_name, v.dependencies));
                });
            }
        }

        public override async Task InstallPackageFiles(PackageGroup package, PV version, string packageDirectory)
        {
            var tsPackage = ThunderstoreAPI.LookupPackage(package.DependencyId).First();
            var tsPackageVersion = tsPackage.versions.First(tspv => tspv.version_number.Equals(version.version));
            var filePath = Path.Combine(packageDirectory, $"{tsPackageVersion.full_name}.zip");

            ThunderstoreAPI.DownloadPackage(tsPackageVersion, filePath);

            while (!File.Exists(filePath))
                await Task.Delay(1);

            using (var fileStream = File.OpenRead(filePath))
            using (var archive = new ZipArchive(fileStream))
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.ToLower().EndsWith("/") || entry.FullName.ToLower().EndsWith("\\"))
                        continue;

                    var outputPath = Path.Combine(packageDirectory, entry.FullName);
                    var outputDir = Path.GetDirectoryName(outputPath);
                    var fileName = Path.GetFileName(outputPath);
                    if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

                    entry.ExtractToFile(outputPath);
                    if (Path.GetExtension(fileName).Equals(".dll"))
                    {
                        string assemblyPath = outputPath;
                        PackageHelper.WriteAssemblyMetaData(assemblyPath, $"{assemblyPath}.meta");
                    }
                }

            File.Delete(filePath);
        }

    }
}
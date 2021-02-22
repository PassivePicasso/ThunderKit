using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Common.Package;
using ThunderKit.Core.Editor;
using ThunderKit.PackageManager.Editor;
using ThunderKit.PackageManager.Engine;
using ThunderKit.PackageManager.Model;
using UnityEditor;

namespace ThunderKit.Integrations.Thunderstore
{
    using PV = PackageManager.Engine.PackageVersion;
    public class ThunderstoreSource : PackageSource
    {

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            var assetPath = $"Assets/ThunderKitSettings/{typeof(ThunderstoreSource).Name}.asset";
            var source = ScriptableHelper.EnsureAsset<ThunderstoreSource>(assetPath, so => { });
            ThunderKitPackageManager.RegisterPackageSource(source);
        }

        public override string Name => "Thunderstore";

        public override string SourceGroup => "Thunderstore";

        protected override IEnumerable<PackageGroup> GetPackagesInternal(string filter = "")
        {
            var thunderstorePackages = ThunderstoreAPI.LookupPackage(filter).ToList();
            var packages = thunderstorePackages
                .Where(tsp => !tsp.is_deprecated)
                .Where(tsp => !tsp.categories.Contains("Modpacks"))
                .Where(tsp => !tsp.categories.Contains("Tools"))
                .OrderByDescending(tsp => tsp.is_pinned)
                .ThenBy(tsp => tsp.name)
                            .Select(tsp => new PackageGroup
                            {
                                author = tsp.owner,
                                name = tsp.name,
                                package_url = tsp.package_url,
                                version = tsp.latest.version_number,
                                description = tsp.latest.description,
                                dependencyId = tsp.full_name,
                                tags = tsp.categories,
                                versions = tsp.versions.Select(v => new PV { version = v.version_number, dependencyId = v.full_name, dependencies = v.dependencies }).ToArray()
                            });
            return packages;
        }

        public override async Task InstallPackageFiles(PackageGroup package, PV version, string packageDirectory)
        {
            var tsPackage = ThunderstoreAPI.LookupPackage(package.dependencyId).First();
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Editor;
using ThunderKit.PackageManager.Editor;
using ThunderKit.PackageManager.Model;
using UnityEditor;

namespace ThunderKit.Integrations.Thunderstore
{
    using PV = PackageManager.Model.PackageVersion;
    public class ThunderstoreSource : PackageSource
    {
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            var assetPath = $"Assets/ThunderKitSettings/{typeof(ThunderstoreSource).Name}.asset";
            var source = ScriptableHelper.EnsureAsset<ThunderstoreSource>(assetPath, so => { });
            ThunderKitPackageManager.RegisterPackageSource(source);
        }

        public override string GetName() => "Thunderstore";

        public override IEnumerable<PackageGroup> GetPackages(string filter = "")
        {
            var thunderstorePackages = ThunderstoreAPI.LookupPackage(filter);
            var packages = thunderstorePackages
                .Where(tsp => !tsp.is_deprecated)
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
                                dependencies = tsp.latest.dependencies,
                                versions = tsp.versions.Select(v => new PV { version = v.version_number, dependencyId = v.full_name }).ToArray()
                            });
            return packages;
        }

        public override void InstallPackage(PackageGroup package, string version, string packageDirectory)
        {
            var tsPackageVersion = ThunderstoreAPI.LookupPackage(package.dependencyId).First();
            var packageVesion = package.versions.First(pv => pv.version.Equals(version));
            var targetPackage = tsPackageVersion.versions.First(tspv => tspv.version_number.Equals(version));
            var filePath = Path.Combine(packageDirectory, $"{targetPackage.full_name}.zip");
            ThunderstoreAPI.DownloadPackage(targetPackage.download_url, filePath);
        }
    }
}
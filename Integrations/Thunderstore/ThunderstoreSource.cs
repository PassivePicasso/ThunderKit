using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Common.Package;
using ThunderKit.Core.Data;
using ThunderKit.Core.Editor;
using UnityEditor;

namespace ThunderKit.Integrations.Thunderstore
{
    using PV = Core.Data.PackageVersion;
    public class ThunderstoreSource : PackageSource
    {
        private static string CachePath = $"Assets/ThunderKitSettings/{typeof(ThunderstoreSource).Name}.asset";
        [InitializeOnLoadMethod]
        public static async Task Initialize()
        {
            await ThunderstoreAPI.ReloadPages();

            var isNew = false;
            var source = ScriptableHelper.EnsureAsset<ThunderstoreSource>(CachePath, so =>
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

        [MenuItem("Tools/ThunderKit/Regenerate Thunderstore PackageSource")]
        public static async Task Regenerate()
        {
            AssetDatabase.DeleteAsset(CachePath);
            await Initialize();
        }

        public override string Name => "Thunderstore";

        public override string SourceGroup => "Thunderstore";
        protected override string VersionIdToGroupId(string dependencyId) => dependencyId.Substring(0, dependencyId.LastIndexOf("-"));

        protected override void OnLoadPackages()
        {
            var loadedPackages = ThunderstoreAPI.LookupPackage(string.Empty).ToArray();
            var realMods = loadedPackages.Where(tsp => !tsp.categories.Contains("Modpacks")).ToArray();
            var orderByPinThenName = realMods.OrderByDescending(tsp => tsp.is_pinned).ThenBy(tsp => tsp.name).ToArray();
            foreach (var tsp in orderByPinThenName)
            {
                var versions = tsp.versions.Select(v => (v.version_number, v.full_name, v.dependencies));
                AddPackageGroup(tsp.owner, tsp.name, tsp.latest.description, tsp.full_name, tsp.categories, versions);
            }
        }

        public override async Task OnInstallPackageFiles(PackageGroup package, PV version, string packageDirectory)
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
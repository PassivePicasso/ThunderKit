using System.IO;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.Core.Editor;
using UnityEditor;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Readers;

namespace ThunderKit.Integrations.Thunderstore
{
    using PV = Core.Data.PackageVersion;
    public class ThunderstoreSource : PackageSource
    {
        private static string CachePath = $"Assets/ThunderKitSettings/{typeof(ThunderstoreSource).Name}.asset";
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            ThunderstoreAPI.ReloadPages();

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
        public static void Regenerate()
        {
            AssetDatabase.DeleteAsset(CachePath);
            Initialize();
        }

        public override string Name => "Thunderstore";

        public override string SourceGroup => "Thunderstore";
        protected override string VersionIdToGroupId(string dependencyId) => dependencyId.Substring(0, dependencyId.LastIndexOf("-"));

        protected override void OnLoadPackages()
        {
            var loadedPackages = ThunderstoreAPI.LookupPackage(string.Empty);
            var realMods = loadedPackages.Where(tsp => !tsp.categories.Contains("Modpacks"));
            var orderByPinThenName = realMods.OrderByDescending(tsp => tsp.is_pinned).ThenBy(tsp => tsp.name);
            foreach (var tsp in orderByPinThenName)
            {
                var versions = tsp.versions.Select(v => new PackageVersionInfo(v.version_number, v.full_name, v.dependencies));
                AddPackageGroup(tsp.owner, tsp.name, tsp.latest.description, tsp.full_name, tsp.categories, versions);
            }
        }

        public override void OnInstallPackageFiles(PV version, string packageDirectory)
        {
            var tsPackage = ThunderstoreAPI.LookupPackage(version.group.DependencyId).First();
            var tsPackageVersion = tsPackage.versions.First(tspv => tspv.version_number.Equals(version.version));
            var filePath = Path.Combine(packageDirectory, $"{tsPackageVersion.full_name}.zip");

            var asyncOp = ThunderstoreAPI.DownloadPackage(tsPackageVersion, filePath);

            asyncOp.completed += (op) =>
            {
                using (var archive = ArchiveFactory.Open(filePath))
                {
                    foreach (var entry in archive.Entries.Where(entry => entry.IsDirectory))
                        Directory.CreateDirectory(Path.Combine(packageDirectory, entry.Key));

                    var extractOptions = new ExtractionOptions { ExtractFullPath = true, Overwrite = true };
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        entry.WriteToDirectory(packageDirectory, extractOptions);
                }
                File.Delete(filePath);
            };
        }
    }
}
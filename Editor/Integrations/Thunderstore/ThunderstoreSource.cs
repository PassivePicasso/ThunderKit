//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.Threading.Tasks;
//using ThunderKit.Common.Package;
//using ThunderKit.Core.Data;
//using ThunderKit.Core.Editor;
//using UnityEditor;

//namespace ThunderKit.Integrations.Thunderstore
//{
//    using PV = Core.Data.PackageVersion;
//    public class ThunderstoreSource : PackageSource
//    {
//        private static string CachePath = $"Assets/ThunderKitSettings/{typeof(ThunderstoreSource).Name}.asset";
//        [InitializeOnLoadMethod]
//        public static void Initialize()
//        {
//            ThunderstoreAPI.ReloadPages();

//            var isNew = false;
//            var source = ScriptableHelper.EnsureAsset<ThunderstoreSource>(CachePath, so =>
//            {
//                isNew = true;
//            });
//            if (isNew)
//            {
//                source.LoadPackages();
//                source.hideFlags = UnityEngine.HideFlags.NotEditable;
//                EditorUtility.SetDirty(source);
//                AssetDatabase.SaveAssets();
//            }
//        }

//        [MenuItem("Tools/ThunderKit/Regenerate Thunderstore PackageSource")]
//        public static async Task Regenerate()
//        {
//            AssetDatabase.DeleteAsset(CachePath);
//            Initialize();
//        }

//        public override string Name => "Thunderstore";

//        public override string SourceGroup => "Thunderstore";
//        protected override string VersionIdToGroupId(string dependencyId) => dependencyId.Substring(0, dependencyId.LastIndexOf("-"));

//        protected override void OnLoadPackages()
//        {
//            var loadedPackages = ThunderstoreAPI.LookupPackage(string.Empty);
//            var realMods = loadedPackages.Where(tsp => !tsp.categories.Contains("Modpacks"));
//            var orderByPinThenName = realMods.OrderByDescending(tsp => tsp.is_pinned).ThenBy(tsp => tsp.name);
//            foreach (var tsp in orderByPinThenName)
//            {
//                var versions = tsp.versions.Select(v => new PackageVersionInfo(v.version_number, v.full_name, v.dependencies));
//                AddPackageGroup(tsp.owner, tsp.name, tsp.latest.description, tsp.full_name, tsp.categories, versions);
//            }
//        }

//        public override void OnInstallPackageFiles(PV version, string packageDirectory)
//        {
//            var tsPackage = ThunderstoreAPI.LookupPackage(version.group.DependencyId).First();
//            var tsPackageVersion = tsPackage.versions.First(tspv => tspv.version_number.Equals(version.version));
//            var filePath = Path.Combine(packageDirectory, $"{tsPackageVersion.full_name}.zip");

//            var asyncOp = ThunderstoreAPI.DownloadPackage(tsPackageVersion, filePath);

//            asyncOp.completed += (op) =>
//            {
//                using (var fileStream = File.OpenRead(filePath))
//                using (var archive = new ZipArchive(fileStream))
//                {
//                    foreach (var entry in archive.Entries)
//                    {
//                        var outputPath = Path.Combine(packageDirectory, entry.FullName);
//                        var outputDir = Path.GetDirectoryName(outputPath);
//                        if (entry.FullName.ToLower().EndsWith("/") || entry.FullName.ToLower().EndsWith("\\"))
//                        {
//                            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
//                            continue;
//                        }

//                        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

//                        entry.ExtractToFile(outputPath);
//                    }
//                }
//                File.Delete(filePath);
//            };
//        }

//        private void AsyncOp_completed(UnityEngine.AsyncOperation obj)
//        {
//            throw new System.NotImplementedException();
//        }
//    }
//}
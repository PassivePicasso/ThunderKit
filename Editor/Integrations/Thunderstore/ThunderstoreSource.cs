using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using ThunderKit.Core;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore
{
    using PV = Core.Data.PackageVersion;
    public class ThunderstoreSource : PackageSource
    {

        const string SettingsPath = "Assets/ThunderKitSettings";
        [InitializeOnLoadMethod]
        static void CreateThunderKitExtensionSource()
        {
            var sources = AssetDatabase.FindAssets($"t:{nameof(PackageSource)}")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<PackageSource>(path))
                .OfType<ThunderstoreSource>()
                .ToArray();

            if (!sources.Any(s => s.Url.ToLower().Contains("thunderkit.thunderstore.io")))
            {
                var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{SettingsPath}/ThunderKit Extensions.asset");
                var asset = ScriptableHelper.EnsureAsset(assetPath, typeof(ThunderstoreSource), asset =>
                {
                    var source = asset as ThunderstoreSource;
                    source.Url = "https://thunderkit.thunderstore.io";
                }) as UnityEngine.Object;
                EditorUtility.SetDirty(asset);
            }
        }

        [Serializable]
        public struct SDateTime
        {
            public long ticks;
            public SDateTime(long ticks)
            {
                this.ticks = ticks;
            }
            public static implicit operator DateTime(SDateTime sdt) => new DateTime(sdt.ticks);
            public static implicit operator SDateTime(DateTime sdt) => new SDateTime(sdt.Ticks);
        }

        class GZipWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                return request;
            }
        }
        internal class ThunderstoreLoadBehaviour : MonoBehaviour { }

        private PackageListing[] packageListings;

        public string Url = "https://thunderstore.io";

        public override string Name => "Thunderstore Source";
        public string PackageListApi => Url + "/api/v1/package/";
        public override string SourceGroup => "Thunderstore";

        private void OnEnable()
        {
            InitializeSources -= Initialize;
            InitializeSources += Initialize;
        }
        private void OnDisable()
        {
            InitializeSources -= Initialize;
        }
        private void OnDestroy()
        {
            InitializeSources -= Initialize;
        }

        private void Initialize(object sender, EventArgs e)
        {
            ReloadPages();
        }

        protected override string VersionIdToGroupId(string dependencyId) => dependencyId.Substring(0, dependencyId.LastIndexOf("-"));

        protected override void OnLoadPackages()
        {
            var realMods = packageListings.Where(tsp => !tsp.categories.Contains("Modpacks"));
            //var orderByPinThenName = realMods.OrderByDescending(tsp => tsp.is_pinned).ThenBy(tsp => tsp.name);
            foreach (var tsp in realMods)
            {
                var versions = tsp.versions.Select(v => new PackageVersionInfo(v.version_number, v.full_name, v.dependencies));
                AddPackageGroup(tsp.owner, tsp.name, tsp.Latest.description, tsp.full_name, tsp.categories, versions);
            }


            SourceUpdated();
        }

        protected override void OnInstallPackageFiles(PV version, string packageDirectory)
        {
            var tsPackage = LookupPackage(version.group.DependencyId).First();
            var tsPackageVersion = tsPackage.versions.First(tspv => tspv.version_number.Equals(version.version));
            var filePath = Path.Combine(packageDirectory, $"{tsPackageVersion.full_name}.zip");

            using (var client = new WebClient())
            {
                client.DownloadFile(tsPackageVersion.download_url, filePath);
            }

            using (var archive = ArchiveFactory.Open(filePath))
            {
                foreach (var entry in archive.Entries.Where(entry => entry.IsDirectory))
                {
                    var path = Path.Combine(packageDirectory, entry.Key);
                    Directory.CreateDirectory(path);
                }

                var extractOptions = new ExtractionOptions { ExtractFullPath = true, Overwrite = true };
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    entry.WriteToDirectory(packageDirectory, extractOptions);
            }

            File.Delete(filePath);
        }

        public void ReloadPages(bool force = false)
        {
            Debug.Log($"Updating Package listing: {PackageListApi}");
            using (var client = new GZipWebClient())
            {
                client.DownloadStringCompleted += Client_DownloadStringCompleted;
                var address = new Uri(PackageListApi);
                client.DownloadStringAsync(address);
            }
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            var json = $"{{ \"{nameof(PackagesResponse.results)}\": {e.Result} }}";
            var response = JsonUtility.FromJson<PackagesResponse>(json);
            packageListings = response.results;
            LoadPackages();
        }

        public IEnumerable<PackageListing> LookupPackage(string name)
        {
            if (packageListings.Length == 0)
                return Enumerable.Empty<PackageListing>();
            else
                return packageListings.Where(package => IsMatch(package, name)).ToArray();
        }

        bool IsMatch(PackageListing package, string name)
        {
            CompareInfo comparer = CultureInfo.CurrentCulture.CompareInfo;
            var compareOptions = CompareOptions.IgnoreCase;
            var nameMatch = comparer.IndexOf(package.name, name, compareOptions) >= 0;
            var fullNameMatch = comparer.IndexOf(package.full_name, name, compareOptions) >= 0;

            var latest = package.versions.OrderByDescending(pck => pck.version_number).First();
            var latestFullNameMatch = comparer.IndexOf(latest.full_name, name, compareOptions) >= 0;
            return nameMatch || fullNameMatch || latestFullNameMatch;
        }

    }
}
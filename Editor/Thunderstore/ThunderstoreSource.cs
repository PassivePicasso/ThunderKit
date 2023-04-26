using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore
{
    using PV = Core.Data.PackageVersion;
    public class ThunderstoreSource : PackageSource
    {

        const string SettingsPath = "Assets/ThunderKitSettings";
        [InitializeOnLoadMethod]
        static void CreateThunderKitExtensionSource() => EditorApplication.update += EnsureThunderKitExtensions;
        private static void EnsureThunderKitExtensions()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }
            
            EditorApplication.update -= EnsureThunderKitExtensions;

            var basePath = $"{SettingsPath}/ThunderKit Extensions.asset";
            var source = AssetDatabase.LoadAssetAtPath<ThunderstoreSource>(basePath);
            if (!source)
            {
                if (File.Exists(basePath))
                    File.Delete(basePath);

                source = ScriptableHelper.EnsureAsset(basePath, typeof(ThunderstoreSource), asset =>
                {
                    var src = asset as ThunderstoreSource;
                    src.Url = "https://thunderkit.thunderstore.io";
                    EditorUtility.SetDirty(src);
                }) as ThunderstoreSource;
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


        protected override string VersionIdToGroupId(string dependencyId) => dependencyId.Substring(0, dependencyId.LastIndexOf("-"));

        protected override void OnLoadPackages()
        {
            var realMods = packageListings.Where(tsp => !tsp.categories.Contains("Modpacks"));
            //var orderByPinThenName = realMods.OrderByDescending(tsp => tsp.is_pinned).ThenBy(tsp => tsp.name);
            foreach (var tsp in realMods)
            {
                var versions = tsp.versions.Select(v => new PackageVersionInfo(v.version_number, v.full_name, v.dependencies, ConstructMarkdown(v, tsp)));
                AddPackageGroup(new PackageGroupInfo
                {
                    Author = tsp.owner,
                    Name = tsp.name,
                    Description = tsp.Latest.description,
                    DependencyId = tsp.name,
                    HeaderMarkdown = $"![]({tsp.Latest.icon}){{ .icon }} {tsp.name}{{ .icon-title .header-1 }}",
                    FooterMarkdown = $"",
                    Versions = versions,
                    Tags = tsp.categories
                });
            }
            SourceUpdated();
        }

        private static string ConstructMarkdown(PackageVersion pv, PackageListing pl)
        {
            var markdown = $"{pv.description}\r\n\r\n";

            if (!string.IsNullOrWhiteSpace(pl.package_url))
                markdown += $"{{ .links }}";

            if (!string.IsNullOrWhiteSpace(pl.package_url)) markdown += $"[Thunderstore]({pl.package_url})";

            return markdown;
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

        protected override async Task ReloadPagesAsyncInternal()
        {
            using (var client = new GZipWebClient())
            {
                var address = new Uri(PackageListApi);
                var result = await client.DownloadStringTaskAsync(address);
                var json = $"{{ \"{nameof(PackagesResponse.results)}\": {result} }}";
                var response = JsonUtility.FromJson<PackagesResponse>(json);
                packageListings = response.results;
                LoadPackages();
            }
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
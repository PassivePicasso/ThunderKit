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
using UnityEngine;

namespace ThunderKit.Integrations.SpaceDock
{
    using PV = Core.Data.PackageVersion;
    public class SpaceDockSource : PackageSource
    {
        enum OrderBy { name, updated, created }
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

        public override string Name => "SpaceDock Source";
        public override string SourceGroup => "SpaceDock";

        protected override string VersionIdToGroupId(string dependencyId) => dependencyId.Substring(0, dependencyId.LastIndexOf("-"));

        protected override void OnLoadPackages()
        {
            //var orderByPinThenName = realMods.OrderByDescending(tsp => tsp.is_pinned).ThenBy(tsp => tsp.name);
            foreach (var tsp in packageListings)
            {
                var versions = tsp.versions.Select(v => new PackageVersionInfo(v.friendly_version, tsp.name, Array.Empty<string>()));
                AddPackageGroup(tsp.author, tsp.name, tsp.short_description, tsp.name, Array.Empty<string>(), versions);
            }
            SourceUpdated();
        }

        protected override void OnInstallPackageFiles(PV version, string packageDirectory)
        {
            var tsPackage = LookupPackage(version.group.DependencyId).First();
            var tsPackageVersion = tsPackage.versions.First(tspv => tspv.friendly_version.Equals(version.version));
            var filePath = Path.Combine(packageDirectory, $"{tsPackage.name}-{tsPackageVersion.friendly_version}.zip");

            using (var client = new WebClient())
            {
                client.DownloadFile(tsPackageVersion.download_path, filePath);
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

        private string PackageListApi(int count, int page, OrderBy orderby)
            => $"https://spacedock.info/api/browse?count={count}&page={page}&orderby={orderby}";

        protected override async Task ReloadPagesAsyncInternal()
        {
            const int PageCount = 500;
            var aggregate = Enumerable.Empty<PackageListing>();
            var tasks = new List<Task<string>>();
            var clients = new List<GZipWebClient>();
            PackagesResponse firstResponse;
            using (var client = new GZipWebClient())
            {
                var address = new Uri(PackageListApi(PageCount, 1, OrderBy.name));
                var firstPage = await client.DownloadStringTaskAsync(address);
                firstResponse = JsonUtility.FromJson<PackagesResponse>(firstPage);
                aggregate = aggregate.Union(firstResponse.result.Where(pl =>
                {
                    if (pl.game_id == 22407) return true;
                    return false;
                }));
            }

            for (int p = 2; p <= firstResponse.pages; p++)
            {
                var client = new GZipWebClient();
                clients.Add(client);
                var address = new Uri(PackageListApi(PageCount, p, OrderBy.name));
                var task = client.DownloadStringTaskAsync(address);
                tasks.Add(task);
            }
            while (tasks.Any())
            {
                foreach (var request in tasks.ToArray())
                {
                    if (!request.IsCompleted) continue;
                    var result = await request;
                    tasks.Remove(request);
                    var response = JsonUtility.FromJson<PackagesResponse>(result);
                    aggregate = aggregate.Union(response.result.Where(pl =>
                    {
                        if (pl.game_id == 22407) return true;
                        return false;
                    }));
                }
                await Task.Delay(100);
            }
            foreach (var client in clients) client.Dispose();

            packageListings = aggregate.ToArray();
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
            var fullNameMatch = comparer.IndexOf(package.name, name, compareOptions) >= 0;

            var latest = package.versions.OrderByDescending(pck => pck.id).First();
            var latestFullNameMatch = comparer.IndexOf(package.name, name, compareOptions) >= 0;
            return nameMatch || fullNameMatch || latestFullNameMatch;
        }
    }
}
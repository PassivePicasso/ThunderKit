﻿using SharpCompress.Archives;
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
            foreach (var tsp in packageListings)
            {
                var versions = tsp.versions.OrderByDescending(v => v.id).Select(v => new PackageVersionInfo(v.friendly_version, tsp.name, Array.Empty<string>(), ConstructMarkdown(v, tsp))).ToArray();

                var header = string.Empty;
                if (!string.IsNullOrEmpty(tsp.background))
                    header += $"![]({tsp.background}){{ .background }}\r\n\r\n";
                header += $"{tsp.name}{{ .background-title .header-1 }}";

                var changeLog = $"### ChangeLog\r\n\r\n{tsp.versions[0].changelog}";

                AddPackageGroup(new PackageGroupInfo
                {
                    Author = tsp.author,
                    Name = tsp.name,
                    Description = tsp.short_description,
                    DependencyId = tsp.name,
                    HeaderMarkdown = header,
                    FooterMarkdown = changeLog,
                    Versions = versions
                });
            }
            SourceUpdated();
        }

        private static string ConstructMarkdown(PackageVersion pv, PackageListing pl)
        {
            var markdown = $"### Description\r\n\r\n{pl.short_description}\r\n\r\n";

            if (!string.IsNullOrWhiteSpace(pl.source_code) || !string.IsNullOrWhiteSpace(pl.website))
                markdown += $"{{ .links }}";

            if (!string.IsNullOrWhiteSpace(pl.source_code)) markdown += $"[Repository]({pl.source_code})";
            if (!string.IsNullOrWhiteSpace(pl.website)) markdown += $"[Website]({pl.website})";

            markdown += $"\r\n\r\n{{ .license }} License: {pl.license}";

            return markdown;
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

        private string PackageListApi(int count, int page, int gameId, OrderBy orderby)
            => $"https://spacedock.info/api/browse?count={count}&page={page}&orderby={orderby}&game_id={gameId}";

        protected override async Task ReloadPagesAsyncInternal()
        {
            const int PageCount = 500;
            var aggregate = Enumerable.Empty<PackageListing>();
            var tasks = new List<Task<string>>();
            var clients = new List<GZipWebClient>();
            PackagesResponse firstResponse;
            using (var client = new GZipWebClient())
            {
                var address = new Uri(PackageListApi(PageCount, 1, 22407, OrderBy.name));
                var firstPage = await client.DownloadStringTaskAsync(address);
                firstResponse = JsonUtility.FromJson<PackagesResponse>(firstPage);
                aggregate = aggregate.Union(firstResponse.result);
            }

            for (int p = 2; p <= firstResponse.pages; p++)
            {
                var client = new GZipWebClient();
                clients.Add(client);
                var address = new Uri(PackageListApi(PageCount, p, 22407, OrderBy.name));
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
                    aggregate = aggregate.Union(response.result);
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
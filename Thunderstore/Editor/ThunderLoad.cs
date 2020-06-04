using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Editor
{
    public class ThunderLoad
    {
        const string ThunderstoreIO = "https://thunderstore.io";
        const string PackageListApi = ThunderstoreIO + "/api/v2/package";
        const string PackageApi = ThunderstoreIO + "/package/download";

        internal static List<Page> loadedPages = new List<Page>();

        public static async Task<IEnumerable<Package>> LookupPackage(string name, int pageIndex = 1, bool isCaseSensitive = true)
        {
            using (WebClient client = new WebClient())
            {
                Debug.Log($"Looking up {name}");

                Uri address = new Uri($"{PackageListApi}/?page={pageIndex}");

                Page page;
                if (loadedPages.Count > pageIndex)
                    page = loadedPages[pageIndex];
                else
                {
                    var response = await client.DownloadStringTaskAsync(address);

                    page = JsonUtility.FromJson<Page>(response);
                    if (page == null || page.count == 0)
                    {
                        Debug.Log("No Thunderstore results found");
                        return null;
                    }
                    loadedPages.Add(page);
                }

                CompareInfo comparer = CultureInfo.CurrentCulture.CompareInfo;
                var compareOptions = isCaseSensitive ? CompareOptions.None : CompareOptions.IgnoreCase;
                var targetPackage = page.results.Where(package =>
                {
                    var nameMatch = comparer.IndexOf(package.name, name, compareOptions) >= 0;
                    var fullNameMatch = comparer.IndexOf(package.full_name, name, compareOptions) >= 0;
                    var latestFullNameMatch = comparer.IndexOf(package.latest.full_name, name, compareOptions) >= 0;
                    return nameMatch || fullNameMatch || latestFullNameMatch;
                });

                return targetPackage == null ? await LookupPackage(name) : targetPackage;
            }
        }

        public static Task<string> DownloadPackageAsync(Package package, string filePath)
        {
            using (WebClient client = new WebClient())
            {
                var latest = package.latest;
                var url = $"{PackageApi}/{package.owner}/{package.name}/{latest.version_number}/";

                return client.DownloadFileTaskAsync(url, filePath).ContinueWith(t => filePath);
            }
        }
        public static Task<string> DownloadPackageAsync((string owner, string name, string version_number) package, string filePath)
        {
            using (WebClient client = new WebClient())
            {
                var url = $"{PackageApi}/{package.owner}/{package.name}/{package.version_number}/";

                return client.DownloadFileTaskAsync(url, filePath).ContinueWith(t => filePath);
            }
        }
    }
}

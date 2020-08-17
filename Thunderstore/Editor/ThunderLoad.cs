using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Editor
{
    public class ThunderLoad
    {
        const string ThunderstoreIO = "https://thunderstore.io";
        const string PackageListApi = ThunderstoreIO + "/api/v2/package";
        const string PackageApi = ThunderstoreIO + "/package/download";

        internal static List<Page> loadedPages = new List<Page>();
        internal static List<Package> loadedPackages = new List<Package>();

        [MenuItem("ThunderKit/Refresh Thunderstore")]
        [InitializeOnLoadMethod]
        public static async void LoadPages()
        {
            loadedPackages.Clear();
            loadedPages.Clear();
            using (WebClient client = new WebClient())
            {
                Page page;
                int pageIndex = 0;
                do
                {
                    try
                    {
                        pageIndex++;
                        Uri address = new Uri($"{PackageListApi}/?page={pageIndex}");
                        var response = await client.DownloadStringTaskAsync(address);
                        page = JsonUtility.FromJson<Page>(response);
                        if (page == null || page.count == 0)
                        {
                            Debug.Log("No Thunderstore results found");
                            return;
                        }
                        loadedPages.Add(page);
                        loadedPackages.AddRange(page.results);
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                } while (true);
            }
            //Debug.Log(loadedPackages.Select(pkg => pkg.latest.full_name).Aggregate((a, b) => $"{a}{Environment.NewLine}{b}"));
        }

        public static IEnumerable<Package> LookupPackage(string name, int pageIndex = 1, bool logStart = true) => loadedPackages.Where(package => IsMatch(package, name)).ToArray();

        static bool IsMatch(Package package, string name)
        {
            CompareInfo comparer = CultureInfo.CurrentCulture.CompareInfo;
            var compareOptions = CompareOptions.IgnoreCase;
            var nameMatch = comparer.IndexOf(package.name, name, compareOptions) >= 0;
            var fullNameMatch = comparer.IndexOf(package.full_name, name, compareOptions) >= 0;
            var latestFullNameMatch = comparer.IndexOf(package.latest.full_name, name, compareOptions) >= 0;
            return nameMatch || fullNameMatch || latestFullNameMatch;
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
    }
}

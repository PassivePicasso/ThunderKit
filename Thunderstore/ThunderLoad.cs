#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore
{
    public class ThunderLoad
    {
        const string ThunderstoreIO = "https://thunderstore.io";
        const string PackageListApi = ThunderstoreIO + "/api/v1/package";

        internal static List<Package> loadedPackages = new List<Package>();

        [MenuItem(ScriptableHelper.ThunderKitMenuRoot + "Refresh Thunderstore")]
        [InitializeOnLoadMethod]
        public static async void LoadPages()
        {
            loadedPackages.Clear();
            using (WebClient client = new WebClient())
            {
                var address = new Uri(PackageListApi);
                var response = await client.DownloadStringTaskAsync(address);
                var resultSet = JsonUtility.FromJson<PackagesResponse>($"{{ \"{nameof(PackagesResponse.results)}\": {response} }}");
                loadedPackages.AddRange(resultSet.results);
            }
        }

        public static IEnumerable<Package> LookupPackage(string name, int pageIndex = 1, bool logStart = true) => loadedPackages.Where(package => IsMatch(package, name)).ToArray();

        static bool IsMatch(Package package, string name)
        {
            CompareInfo comparer = CultureInfo.CurrentCulture.CompareInfo;
            var compareOptions = CompareOptions.IgnoreCase;
            var nameMatch = comparer.IndexOf(package.name, name, compareOptions) >= 0;
            var fullNameMatch = comparer.IndexOf(package.full_name, name, compareOptions) >= 0;

            var latest = package.versions.OrderByDescending(pck => pck.version_number).First();
            var latestFullNameMatch = comparer.IndexOf(latest.full_name, name, compareOptions) >= 0;
            return nameMatch || fullNameMatch || latestFullNameMatch;
        }

        public static Task<string> DownloadPackageAsync(Package package, string filePath)
        {
            using (WebClient client = new WebClient())
            {
                var latest = package.versions.OrderByDescending(pck => pck.version_number).First();

                return client.DownloadFileTaskAsync(latest.download_url, filePath).ContinueWith(t => filePath);
            }
        }
    }
}
#endif
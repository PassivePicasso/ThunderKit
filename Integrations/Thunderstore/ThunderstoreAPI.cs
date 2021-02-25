using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ThunderKit.Common.Package;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore
{
    /// <summary>
    /// ThunderstoreAPI provides an interface to the Thunderstore API
    /// Currently supports Listing, Downloading, and Searching for packages.
    /// </summary>
    public class ThunderstoreAPI
    {
        static string PackageListApi => ThunderKitSetting.GetOrCreateSettings<ThunderstoreSettings>().ThunderstoreUrl + "/api/v1/package";

        internal static List<PackageListing> loadedPackages;

        [MenuItem(Common.Constants.ThunderKitMenuRoot + "Refresh Thunderstore", priority = Common.Constants.ThunderKitMenuPriority)]
        [InitializeOnLoadMethod]
        public static void LoadPages()
        {
            ThunderstoreSettings.OnThunderstoreUrlChanged -= LoadPages;
            ThunderstoreSettings.OnThunderstoreUrlChanged += LoadPages;
            _ = ReloadPages();
        }

        static double timeSinceStartup;

        public static void LoadPages(object sender, (string newValue, string oldValue) value)
        {
            timeSinceStartup = EditorApplication.timeSinceStartup;
            EditorApplication.update -= WaitUpdate;
            EditorApplication.update += WaitUpdate;
        }

        private static void WaitUpdate()
        {
            var timeElapsed = EditorApplication.timeSinceStartup - timeSinceStartup;
            if (timeElapsed > 2)
            {
                EditorApplication.update -= WaitUpdate;
                _ = ReloadPages();
            }
        }

        public static async Task ReloadPages()
        {
            var packages = new List<PackageListing>();
            using (WebClient client = new WebClient())
            {
                var address = new Uri(PackageListApi);
                var response = await client.DownloadStringTaskAsync(address);
                var resultSet = JsonUtility.FromJson<PackagesResponse>($"{{ \"{nameof(PackagesResponse.results)}\": {response} }}");
                var nonDeprecatedResults = resultSet.results.Where(package => !package.is_deprecated);
                packages.AddRange(nonDeprecatedResults);
            }
            loadedPackages = packages;
            //Debug.Log($"Package listing update: {PackageListApi}");
        }

        public static IEnumerable<PackageListing> LookupPackage(string name, int pageIndex = 1, bool logStart = true) => loadedPackages.Where(package => IsMatch(package, name)).ToArray();

        static bool IsMatch(PackageListing package, string name)
        {
            CompareInfo comparer = CultureInfo.CurrentCulture.CompareInfo;
            var compareOptions = CompareOptions.IgnoreCase;
            var nameMatch = comparer.IndexOf(package.name, name, compareOptions) >= 0;
            var fullNameMatch = comparer.IndexOf(package.full_name, name, compareOptions) >= 0;

            var latest = package.versions.OrderByDescending(pck => pck.version_number).First();
            var latestFullNameMatch = comparer.IndexOf(latest.full_name, name, compareOptions) >= 0;
            return nameMatch || fullNameMatch || latestFullNameMatch;
        }

        public static Task<string> DownloadPackageAsync(PackageListing package, string filePath)
        {
            using (WebClient WebClient = new WebClient())
            {
                var latest = package.versions.OrderByDescending(pck => pck.version_number).First();

                return WebClient.DownloadFileTaskAsync(latest.download_url, filePath).ContinueWith(t => filePath);
            }
        }

        public static void DownloadLatestPackage(PackageListing package, string filePath)
        {
            DownloadPackage(package.versions.OrderByDescending(pck => pck.version_number).First(), filePath);
        }

        public static void DownloadPackage(PackageVersion package, string filePath)
        {
            PackageHelper.DownloadPackage(package.download_url, filePath);
        }

    }
}
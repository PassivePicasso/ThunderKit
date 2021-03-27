using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using ThunderKit.Common.Package;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static ThunderKit.Integrations.Thunderstore.ThunderstoreSettings;

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

        [InitializeOnLoadMethod]
        public static void LoadPages()
        {
            ThunderstoreSettings.OnThunderstoreUrlChanged -= LoadPages;
            ThunderstoreSettings.OnThunderstoreUrlChanged += LoadPages;
            ReloadPages();
        }

        static double timeSinceStartup;

        public static void LoadPages(object sender, StringValueChangeArgs value)
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
                ReloadPages();
            }
        }

        public static void ReloadPages()
        {
            var packages = new List<PackageListing>();
            var webRequest = UnityWebRequest.Get(PackageListApi);
            var asyncOpRequest = webRequest.SendWebRequest();
            
            asyncOpRequest.completed += (obj) =>
            {
                var response = string.Empty;

#if UNITY_2020_1_OR_NEWER
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
#else
                if (webRequest.isNetworkError || webRequest.isHttpError)
#endif
                    Debug.Log(webRequest.error);
                else
                    response = webRequest.downloadHandler.text;

                var resultSet = JsonUtility.FromJson<PackagesResponse>($"{{ \"{nameof(PackagesResponse.results)}\": {response} }}");
                packages.AddRange(resultSet.results);
                loadedPackages = packages;
                Debug.Log($"Package listing update: {PackageListApi}");
            };
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

        public static void DownloadLatestPackage(PackageListing package, string filePath)
        {
            DownloadPackage(package.versions.OrderByDescending(pck => pck.version_number).First(), filePath);
        }

        public static UnityWebRequestAsyncOperation DownloadPackage(PackageVersion package, string filePath)
        {
            return PackageHelper.DownloadPackage(package.download_url, filePath);
        }
    }
}
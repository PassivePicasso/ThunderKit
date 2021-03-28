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

namespace ThunderKit.Integrations.Thunderstore
{
    /// <summary>
    /// ThunderstoreAPI provides an interface to the Thunderstore API
    /// Currently supports Listing, Downloading, and Searching for packages.
    /// </summary>
    public class ThunderstoreAPI
    {
        static string PackageListApi => ThunderKitSetting.GetOrCreateSettings<ThunderstoreSettings>().ThunderstoreUrl + "/api/v1/package";

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
                var settings = ThunderKitSetting.GetOrCreateSettings<ThunderstoreSettings>();
                settings.LoadedPages = packages;
                EditorUtility.SetDirty(settings);
                Debug.Log($"Package listing update: {PackageListApi}");
            };
        }

        public static IEnumerable<PackageListing> LookupPackage(string name)
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderstoreSettings>();
            return settings.LoadedPages.Where(package => IsMatch(package, name)).ToArray();
        }

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
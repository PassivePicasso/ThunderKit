using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public static List<PackageListing> LoadedPages;

        static string PackageListApi => ThunderKitSetting.GetOrCreateSettings<ThunderstoreSettings>().ThunderstoreUrl + "/api/v1/package/";

        public static void ReloadPages()
        {
            Debug.Log($"Updating Package listing against {PackageListApi}");
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
                {
                    response = webRequest.downloadHandler.text;

                    var resultSet = JsonUtility.FromJson<PackagesResponse>($"{{ \"{nameof(PackagesResponse.results)}\": {response} }}");
                    packages.AddRange(resultSet.results);
                    LoadedPages = packages;
                    Debug.Log($"Package listing update: {PackageListApi}");
                }
            };
        }

        public static IEnumerable<PackageListing> LookupPackage(string name)
        {
            if (LoadedPages == null || LoadedPages.Count == 0)
                return Enumerable.Empty<PackageListing>();
            else
                return LoadedPages.Where(package => IsMatch(package, name)).ToArray();
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

    }
}
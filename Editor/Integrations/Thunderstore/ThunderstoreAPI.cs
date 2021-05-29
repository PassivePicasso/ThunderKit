using System;
using System.Collections;
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
        internal class ThunderstoreLoadBehaviour : MonoBehaviour { }

        private static PackageListing[] packageListings;
        private static UnityWebRequest webRequest;
        private static UnityWebRequestAsyncOperation request;

        public static IEnumerable<PackageListing> PackageListings => packageListings.ToList();

        static string PackageListApi => ThunderKitSetting.GetOrCreateSettings<ThunderstoreSettings>().ThunderstoreUrl + "/api/v1/package/";
        public static event EventHandler PagesLoaded;

        public static void ReloadPages()
        {
            if (request != null)
            {
                request.completed -= Completed;
                if (webRequest != null)
                {
                    webRequest?.Dispose();
                }
            }
            Debug.Log($"Updating Package listing: {PackageListApi}");
            webRequest = UnityWebRequest.Get(PackageListApi);
            request = webRequest.SendWebRequest();
            request.completed += Completed;
        }

        static void Completed(AsyncOperation aop)
        {
            if (!(aop is UnityWebRequestAsyncOperation requestOperation)) return;
            request.completed -= Completed;
#if UNITY_2020_1_OR_NEWER
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
#else
            if (webRequest.isNetworkError || webRequest.isHttpError)
#endif
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                var json = $"{{ \"{nameof(PackagesResponse.results)}\": {webRequest.downloadHandler.text} }}";
                var response = JsonUtility.FromJson<PackagesResponse>(json);
                packageListings = response.results;
                PagesLoaded?.Invoke(null, EventArgs.Empty);
                Debug.Log($"Package listing updated: {PackageListApi}");

            }
            webRequest.Dispose();
        }

        public static IEnumerable<PackageListing> LookupPackage(string name)
        {
            if (packageListings.Length == 0)
                return Enumerable.Empty<PackageListing>();
            else
                return packageListings.Where(package => IsMatch(package, name)).ToArray();
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
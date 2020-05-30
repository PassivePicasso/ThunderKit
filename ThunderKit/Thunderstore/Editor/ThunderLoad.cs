using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace RainOfStages.Thunderstore
{
    public class ThunderLoad
    {
        const string ThunderstoreIO = "https://thunderstore.io";
        const string PackageListApi = ThunderstoreIO + "/api/v2/package";
        const string PackageApi = ThunderstoreIO + "/package/download";

        internal static List<Page> loadedPages = new List<Page>();

        public static async Task<Package> LookupPackage(string name, int pageIndex = 1)
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

                var targetPackage = page.results.FirstOrDefault(package => package.name.Contains(name));

                return targetPackage == null ? await LookupPackage(name) : targetPackage;
            }
        }

        public static Task DownloadPackageAsync(Package package, string filePath)
        {
            using (WebClient client = new WebClient())
            {
                var latest = package.latest;
                var url = $"{PackageApi}/{package.owner}/{package.name}/{latest.version_number}/";

                return client.DownloadFileTaskAsync(url, filePath);
            }
        }
    }
}

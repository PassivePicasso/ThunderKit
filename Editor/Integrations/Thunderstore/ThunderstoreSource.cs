using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ThunderKit.Integrations.Thunderstore
{
    using PV = Core.Data.PackageVersion;
    public class ThunderstoreSource : PackageSource
    {
        struct DownloadData : IEquatable<DownloadData>
        {
            public readonly Guid guid;
            public readonly string FilePath;
            public readonly string PackageDirectory;
            public readonly UnityWebRequestAsyncOperation AsyncOperation;

            public DownloadData(string filePath, string packageDirectory, UnityWebRequestAsyncOperation asyncOperation)
            {
                this.guid = Guid.NewGuid();
                FilePath = filePath;
                PackageDirectory = packageDirectory;
                AsyncOperation = asyncOperation;
            }

            public override bool Equals(object obj)
            {
                return obj is DownloadData data && Equals(data);
            }

            public bool Equals(DownloadData other)
            {
                return guid.Equals(other.guid);
            }

            public override int GetHashCode()
            {
                return -1324198676 + guid.GetHashCode();
            }

            public static bool operator ==(DownloadData left, DownloadData right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(DownloadData left, DownloadData right)
            {
                return !(left == right);
            }
        }

        private readonly static string CachePath = $"Assets/ThunderKitSettings/{typeof(ThunderstoreSource).Name}.asset";

        [InitializeOnLoadMethod]
        public static void SetupInitialization()
        {
            InitializeSources -= PackageSource_InitializeSources;
            InitializeSources += PackageSource_InitializeSources;
        }

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            AssetDatabase.DeleteAsset(CachePath);
            ThunderstoreAPI.ReloadPages();

            var isNew = false;
            var source = ScriptableHelper.EnsureAsset<ThunderstoreSource>(CachePath, so =>
            {
                isNew = true;
            });
            if (isNew)
            {
                source.hideFlags = UnityEngine.HideFlags.NotEditable;
                source.LoadPackages();
                EditorApplication.update += ProcessDownloads;
            }
        }

        private static void PackageSource_InitializeSources(object sender, System.EventArgs e)
        {
            Initialize();
        }

        public override string Name => "Thunderstore";

        public override string SourceGroup => "Thunderstore";
        protected override string VersionIdToGroupId(string dependencyId) => dependencyId.Substring(0, dependencyId.LastIndexOf("-"));

        protected override void OnLoadPackages()
        {
            var loadedPackages = ThunderstoreAPI.LookupPackage(string.Empty);
            var realMods = loadedPackages.Where(tsp => !tsp.categories.Contains("Modpacks"));
            var orderByPinThenName = realMods.OrderByDescending(tsp => tsp.is_pinned).ThenBy(tsp => tsp.name);
            foreach (var tsp in orderByPinThenName)
            {
                var versions = tsp.versions.Select(v => new PackageVersionInfo(v.version_number, v.full_name, v.dependencies));
                AddPackageGroup(tsp.owner, tsp.name, tsp.Latest.description, tsp.full_name, tsp.categories, versions);
            }
        }


        private static List<DownloadData> ActiveOperations = new List<DownloadData>();

        public override void OnInstallPackageFiles(PV version, string packageDirectory)
        {
            var tsPackage = ThunderstoreAPI.LookupPackage(version.group.DependencyId).First();
            var tsPackageVersion = tsPackage.versions.First(tspv => tspv.version_number.Equals(version.version));
            var filePath = Path.Combine(packageDirectory, $"{tsPackageVersion.full_name}.zip");

            var asyncOp = ThunderstoreAPI.DownloadPackage(tsPackageVersion, filePath);

            ActiveOperations.Add(new DownloadData(filePath, packageDirectory, asyncOp));
        }

        private static void ProcessDownloads()
        {
            var downloadData = ActiveOperations.FirstOrDefault(dd => dd.AsyncOperation.isDone);
            if (string.IsNullOrEmpty(downloadData.PackageDirectory)) return;
            if (string.IsNullOrEmpty(downloadData.FilePath)) return;
            if (downloadData.AsyncOperation == null) return;
            if (!downloadData.AsyncOperation.isDone) return;

            using (var archive = ArchiveFactory.Open(downloadData.FilePath))
            {
                foreach (var entry in archive.Entries.Where(entry => entry.IsDirectory))
                {
                    var path = Path.Combine(downloadData.PackageDirectory, entry.Key);
                    Directory.CreateDirectory(path);
                }

                var extractOptions = new ExtractionOptions { ExtractFullPath = true, Overwrite = true };
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    entry.WriteToDirectory(downloadData.PackageDirectory, extractOptions);
            }

            File.Delete(downloadData.FilePath);
            ActiveOperations.Remove(downloadData);
            if (ActiveOperations.Count == 0)
                AssetDatabase.Refresh();
        }
    }
}
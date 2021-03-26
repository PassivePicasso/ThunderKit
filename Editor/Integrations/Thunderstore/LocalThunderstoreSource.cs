using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ThunderKit.Common;
using ThunderKit.Common.Package;
using ThunderKit.Core.Data;
using ThunderKit.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore
{
    using PV = Core.Data.PackageVersion;
    public class LocalThunderstoreSource : PackageSource
    {
        private static string CachePath = $"Assets/ThunderKitSettings/{nameof(LocalThunderstoreSource)}.asset";

        [MenuItem(Constants.ThunderKitContextRoot + "Refresh Local Thunderstore sources", priority = Constants.ThunderKitMenuPriority)]
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            var localSources = AssetDatabase.FindAssets($"t:{nameof(LocalThunderstoreSource)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<LocalThunderstoreSource>)
                .Where(source=> source.Packages == null || !source.Packages.Any())
                .ToArray();
            foreach(var drySource in localSources)
                drySource.LoadPackages();

        }

        [MenuItem(Constants.ThunderKitContextRoot + "Create Local Thunderstore PackageSource", priority = Constants.ThunderKitMenuPriority)]
        public static void Create()
        {
            var isNew = false;
            var source = ScriptableHelper.EnsureAsset<LocalThunderstoreSource>(CachePath, so =>
            {
                isNew = true;
            });
            if (isNew)
            {
                EditorUtility.SetDirty(source);
                AssetDatabase.SaveAssets();
            }
        }

        public string LocalRepositoryPath;

        public override string Name => "Local Thunderstore";
        public override string SourceGroup => "Thunderstore";
        protected override string VersionIdToGroupId(string dependencyId) => dependencyId.Substring(0, dependencyId.LastIndexOf("-"));
        protected override void OnLoadPackages()
        {
            var potentialPackages = Directory.GetFiles(LocalRepositoryPath, "*.zip", SearchOption.TopDirectoryOnly);
            foreach (var filePath in potentialPackages)
            {
                using (var fileStream = File.OpenRead(filePath))
                using (var archive = new System.IO.Compression.ZipArchive(fileStream))
                    foreach (var entry in archive.Entries)
                    {
                        if (!"manifest.json".Equals(Path.GetFileName(entry.FullName))) continue;

                        var manifestJson = string.Empty;
                        using (var reader = new StreamReader(entry.Open()))
                            manifestJson = reader.ReadToEnd();

                        var tsp = JsonUtility.FromJson<PackageVersion>(manifestJson);

                        var versionId = Path.GetFileNameWithoutExtension(filePath);
                        var author = versionId.Split('-')[0];
                        var groupId = $"{author}-{tsp.name}";
                        var versions = new[] { (tsp.version_number, versionId, tsp.dependencies) };
                        AddPackageGroup(author, tsp.name, tsp.description, groupId, Array.Empty<string>(), versions);
                        //don't process additional manifest files
                        break;
                    }
            }
        }

        public override async Task OnInstallPackageFiles(PV version, string packageDirectory)
        {
            await Task.Run(() =>
            {
                var potentialPackages = Directory.EnumerateFiles(LocalRepositoryPath, "*.zip", SearchOption.TopDirectoryOnly);
                foreach (var filePath in potentialPackages)
                {
                    using (var fileStream = File.OpenRead(filePath))
                    using (var archive = new ZipArchive(fileStream))
                    {
                        var manifestJsonEntry = archive.Entries.First(entry => entry.Name.Equals("manifest.json"));
                        var manifestJson = string.Empty;
                        using (var reader = new StreamReader(manifestJsonEntry.Open()))
                            manifestJson = reader.ReadToEnd();

                        var version_full_name = Path.GetFileNameWithoutExtension(filePath);
                        var author = version_full_name.Split('-')[0];
                        var manifest = JsonUtility.FromJson<PackageVersion>(manifestJson);
                        var full_name = $"{author}-{manifest.name}";
                        if (full_name != version.group.DependencyId) continue;

                        foreach (var entry in archive.Entries)
                        {
                            var outputPath = Path.Combine(packageDirectory, entry.FullName);
                            var outputDir = Path.GetDirectoryName(outputPath);
                            if (entry.FullName.ToLower().EndsWith("/") || entry.FullName.ToLower().EndsWith("\\"))
                            {
                                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                                continue;
                            }

                            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

                            entry.ExtractToFile(outputPath);
                        }
                    }
                }
            });
        }

    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ThunderKit.Common;
using ThunderKit.Common.Package;
using ThunderKit.Core.Editor;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Manifests.Datum;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Data
{
    public abstract class PackageSource : ScriptableObject, IEquatable<PackageSource>
    {
        public static event EventHandler SourcesInitialized;
        public static event EventHandler InitializeSources;

        public static void LoadAllSources()
        {
            InitializeSources?.Invoke(null, EventArgs.Empty);
            SourcesInitialized?.Invoke(null, EventArgs.Empty);
        }

        [Serializable]
        public class PackageVersionInfo
        {
            public string version;
            public string versionDependencyId;
            public string[] dependencies;

            public PackageVersionInfo(string version, string dependencyId, string[] dependencies)
            {
                this.version = version;
                this.versionDependencyId = dependencyId;
                this.dependencies = dependencies;
            }
        }

        static Dictionary<string, List<PackageSource>> sourceGroups;
        public static Dictionary<string, List<PackageSource>> SourceGroups
        {
            get
            {
                if (sourceGroups == null)
                {
                    sourceGroups = new Dictionary<string, List<PackageSource>>();
                    var packageSources = AssetDatabase.FindAssets("t:PackageSource", new string[] { "Assets", "Packages" })
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Select(AssetDatabase.LoadAssetAtPath<PackageSource>);
                    foreach (var packageSource in packageSources)
                    {
                        if (!sourceGroups.TryGetValue(packageSource.SourceGroup, out var sourceGroup))
                            sourceGroups[packageSource.SourceGroup] = sourceGroup = new List<PackageSource> { packageSource };
                        else if (!sourceGroup.Contains(packageSource))
                            sourceGroup.Add(packageSource);
                    }

                }
                return sourceGroups;
            }
        }



        public DateTime lastUpdateTime;
        public abstract string Name { get; }
        public abstract string SourceGroup { get; }

        public List<PackageGroup> Packages;

        private Dictionary<string, HashSet<string>> dependencyMap;
        private Dictionary<string, PackageGroup> groupMap;

        /// <summary>
        /// Generates a new PackageGroup for this PackageSource
        /// </summary>
        /// <param name="author"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="dependencyId">DependencyId for PackageGroup, this is used for mapping dependencies</param>
        /// <param name="tags"></param>
        /// <param name="versions">Collection of version numbers, DependencyIds and dependencies as an array of versioned DependencyIds</param>
        protected void AddPackageGroup(string author, string name, string description, string dependencyId, string[] tags, IEnumerable<PackageVersionInfo> versions)
        {
            if (groupMap == null) groupMap = new Dictionary<string, PackageGroup>();
            if (dependencyMap == null) dependencyMap = new Dictionary<string, HashSet<string>>();
            if (Packages == null) Packages = new List<PackageGroup>();
            var group = CreateInstance<PackageGroup>();

            group.Author = author;
            group.name = group.PackageName = name;
            group.Description = description;
            group.DependencyId = dependencyId;
            group.Tags = tags;
            group.Source = this;
            groupMap[dependencyId] = group;

            group.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
            AssetDatabase.AddObjectToAsset(group, this);

            var versionData = versions.ToArray();
            group.Versions = new PackageVersion[versionData.Length];
            for (int i = 0; i < versionData.Length; i++)
            {
                var version = versionData[i].version;
                var versionDependencyId = versionData[i].versionDependencyId;
                var dependencies = versionData[i].dependencies;

                var packageVersion = CreateInstance<PackageVersion>();
                packageVersion.name = packageVersion.dependencyId = versionDependencyId;
                packageVersion.group = group;
                packageVersion.version = version;
                packageVersion.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
                AssetDatabase.AddObjectToAsset(packageVersion, group);
                group.Versions[i] = packageVersion;

                if (!dependencyMap.TryGetValue(packageVersion.dependencyId, out var packageDeps))
                    dependencyMap[packageVersion.dependencyId] = packageDeps = new HashSet<string>();

                packageDeps.UnionWith(dependencies);
            }

            Packages.Add(group);
        }

        /// <summary>
        /// Loads data from data source into the current PackageSource via AddPackageGroup
        /// </summary>
        protected abstract void OnLoadPackages();

        /// <summary>
        /// Provides a conversion of versioned dependencyIds to group dependencyIds
        /// </summary>
        /// <param name="dependencyId">Versioned Dependency Id</param>
        /// <returns>Group DependencyId which dependencyId is mapped to</returns>
        protected abstract string VersionIdToGroupId(string dependencyId);


        internal void Clear()
        {
            if (Packages == null) return;
            foreach (var package in Packages)
            {
                AssetDatabase.RemoveObjectFromAsset(package);
                DestroyImmediate(package);
            }
            Packages.Clear();
        }

        public void LoadPackages()
        {
            Clear();
            OnLoadPackages();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var validVersions = Packages.Where(pkgGrp => pkgGrp).Where(pkgGrp => pkgGrp.Versions != null);
            var versionGroupMaps = validVersions.SelectMany(pkgGrp => pkgGrp.Versions.Select(pkgVer => new KeyValuePair<PackageGroup, PackageVersion>(pkgGrp, pkgVer)));
            var versionMap = versionGroupMaps.Distinct().ToDictionary(ver => ver.Value.dependencyId);

            foreach (var packageGroup in Packages)
            {
                foreach (var version in packageGroup.Versions)
                {
                    var dependencies = dependencyMap[version.name].ToArray();
                    version.dependencies = new PackageVersion[dependencies.Length];
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        string dependencyId = dependencies[i];
                        string groupId = VersionIdToGroupId(dependencyId);
                        if (versionMap.TryGetValue(dependencyId, out var packageDep))
                        {
                            version.dependencies[i] = packageDep.Value;
                        }
                        else if (groupMap.TryGetValue(groupId, out var groupDep))
                        {
                            version.dependencies[i] = groupDep["latest"];
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        IEnumerable<PackageVersion> EnumerateDependencies(PackageVersion package)
        {
            foreach (var dependency in package.dependencies)
            {
                foreach (var subDependency in EnumerateDependencies(dependency))
                    yield return subDependency;
            }
            yield return package;
        }

        public void InstallPackage(PackageGroup group, string version)
        {
            if (EditorApplication.isCompiling) return;
            var package = group[version];

            var installSet = EnumerateDependencies(package).Where(dep => !dep.group.Installed).ToArray();
            var progress = 0.01f;
            var stepSize = 0.33f / installSet.Length;
            EditorApplication.LockReloadAssemblies();
            EditorUtility.DisplayProgressBar("Loading Packages", $"{installSet.Length} packages", progress);
            foreach (var installable in installSet)
            {
                EditorUtility.DisplayProgressBar("Loading Packages", $"Downloading {installable.group.PackageName}", progress += stepSize / 2);

                //This will cause repeated installation of dependencies
                string packageDirectory = installable.group.InstallDirectory;

                if (Directory.Exists(packageDirectory)) Directory.Delete(packageDirectory, true);
                Directory.CreateDirectory(packageDirectory);

                installable.group.Source.OnInstallPackageFiles(installable, packageDirectory);

                foreach (var assemblyPath in Directory.GetFiles(packageDirectory, "*.dll", SearchOption.AllDirectories))
                    PackageHelper.WriteAssemblyMetaData(assemblyPath, $"{assemblyPath}.meta");

                EditorUtility.DisplayProgressBar("Loading Packages", $"Creating package.json for {installable.group.PackageName}", progress += stepSize / 2);
                PackageHelper.GeneratePackageManifest(
                      installable.group.DependencyId.ToLower(), installable.group.InstallDirectory,
                      installable.group.PackageName, installable.group.Author,
                      installable.version,
                      installable.group.Description);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("Loading Packages", $"Creating {installSet.Length} manifests", progress);
            foreach (var installable in installSet)
            {
                var installableGroup = installable.group;
                var manifestPath = PathExtensions.Combine(installableGroup.PackageDirectory, $"{installableGroup.PackageName}.asset");
                if (AssetDatabase.LoadAssetAtPath<Manifest>(manifestPath))
                    AssetDatabase.DeleteAsset(manifestPath);

                EditorUtility.DisplayProgressBar("Loading Packages", $"Creating manifest for {installable.group.PackageName}", progress += stepSize);
                var identity = CreateInstance<ManifestIdentity>();
                identity.name = nameof(ManifestIdentity);
                identity.Author = installableGroup.Author;
                identity.Description = installableGroup.Description;
                identity.Name = installableGroup.PackageName;
                identity.Version = version;
                var manifest = ScriptableHelper.EnsureAsset<Manifest>(manifestPath);
                manifest.InsertElement(identity, 0);
                manifest.Identity = identity;
                PackageHelper.WriteAssetMetaData(manifestPath, $"{manifestPath}.meta");
            }

            EditorUtility.DisplayProgressBar("Loading Packages", $"Assigning dependencies for {installSet.Length} manifests", progress);
            foreach (var installable in installSet)
            {
                var manifestAssetPath = PathExtensions.Combine(installable.group.PackageDirectory, $"{installable.group.PackageName}.asset");
                var installableManifest = AssetDatabase.LoadAssetAtPath<Manifest>(manifestAssetPath);
                var identity = installableManifest.Identity;
                EditorUtility.DisplayProgressBar("Loading Packages", $"Assigning dependencies for {installable.group.PackageName}", progress);
                identity.Dependencies = new Manifest[installable.dependencies.Length];
                for (int i = 0; i < installable.dependencies.Length; i++)
                {
                    var installableDependency = installable.dependencies[i];

                    EditorUtility.DisplayProgressBar("Loading Packages",
                                                     $"Assigning {installableDependency.group.PackageName} to {identity.Name}",
                                                     progress += stepSize / installable.dependencies.Length);

                    var manifestFileName = $"{installableDependency.group.PackageName}.asset";
                    var dependencyAssetTempPath = PathExtensions.Combine(installableDependency.group.PackageDirectory, manifestFileName);
                    var manifest = AssetDatabase.LoadAssetAtPath<Manifest>(dependencyAssetTempPath);

                    if (!manifest)
                    {
                        var packageManifests = AssetDatabase.FindAssets($"t:{nameof(Manifest)}", new string[] { "Assets", "Packages" }).Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();
                        manifest = packageManifests.Where(x => x.Contains(manifestFileName)).Select(x => AssetDatabase.LoadAssetAtPath<Manifest>(x)).FirstOrDefault();
                    }

                    identity.Dependencies[i] = manifest;
                }
                EditorUtility.SetDirty(installable);
                EditorUtility.SetDirty(identity);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();

            EditorApplication.update += OneRefresh;

            RefreshWait = EditorApplication.timeSinceStartup;
            EditorApplication.UnlockReloadAssemblies();
        }

        double RefreshWait = 0;
        private void OneRefresh()
        {
            if (EditorApplication.timeSinceStartup - RefreshWait < 1) return;

            EditorApplication.update -= OneRefresh;
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Executes the downloading, unpacking, and placing of package files.
        /// </summary>
        /// <param name="version">The version of the Package which should be installed</param>
        /// <param name="packageDirectory">Root directory which files should be extracted into</param>
        /// <returns></returns>
        protected abstract void OnInstallPackageFiles(PackageVersion version, string packageDirectory);


        public override bool Equals(object obj)
        {
            return Equals(obj as PackageSource);
        }

        public bool Equals(PackageSource other)
        {
            return other != null &&
                   base.Equals(other) &&
                   Name == other.Name &&
                   SourceGroup == other.SourceGroup;
        }

        public override int GetHashCode()
        {
            int hashCode = 1502236599;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SourceGroup);
            return hashCode;
        }

        public static bool operator ==(PackageSource left, PackageSource right)
        {
            return EqualityComparer<PackageSource>.Default.Equals(left, right);
        }

        public static bool operator !=(PackageSource left, PackageSource right)
        {
            return !(left == right);
        }
    }
}
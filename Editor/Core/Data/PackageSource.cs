using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Common;
using ThunderKit.Common.Logging;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Data
{
    public abstract class PackageSource : ScriptableObject, IEquatable<PackageSource>
    {
        public static event EventHandler SourcesInitialized;
        public static event EventHandler InitializeSources;

        public bool IsLoadingPages { get; private set; }
        public event Action OnLoadingStarted;
        public event Action OnLoadingStopped;

        public static void LoadAllSources()
        {
            InitializeSources?.Invoke(null, EventArgs.Empty);
            SourcesInitialized?.Invoke(null, EventArgs.Empty);
        }

        public static void SourceUpdated() => SourcesInitialized?.Invoke(null, EventArgs.Empty);

        [Serializable]
        public class PackageVersionInfo
        {
            public string Version;
            public string VersionDependencyId;
            public string Markdown;
            public string[] Dependencies;

            public PackageVersionInfo(string version, string dependencyId, string[] dependencies, string markdown)
            {
                this.Version = version;
                this.VersionDependencyId = dependencyId;
                this.Dependencies = dependencies;
                Markdown = markdown;
            }
        }
        [Serializable]
        public class PackageGroupInfo
        {
            public string Author;
            public string Name;
            public string Description;
            public string DependencyId;
            public string HeaderMarkdown;
            public string FooterMarkdown;
            public string[] Tags;
            public IEnumerable<PackageVersionInfo> Versions;
        }

        static Dictionary<string, List<PackageSource>> sourceGroups;
        public static Dictionary<string, List<PackageSource>> SourceGroups
        {
            get
            {
                if (sourceGroups == null)
                {
                    sourceGroups = new Dictionary<string, List<PackageSource>>();
                    foreach (var packageSource in PackageSourceSettings.PackageSources)
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

        public List<PackageGroup> Packages
        {
            get
            {
                if (packages == null)
                    packages = new List<PackageGroup>();

                return packages;
            }

            private set => packages = value;
        }

        private Dictionary<string, HashSet<string>> dependencyMap;
        private Dictionary<string, PackageGroup> groupMap;
        private List<PackageGroup> packages;
        private ThunderKitSettings settings;

        private void OnEnable()
        {
            InitializeSources -= Initialize;
            InitializeSources += Initialize;
            settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
        }

        private void OnDisable()
        {
            InitializeSources -= Initialize;
        }

        private void OnDestroy()
        {
            InitializeSources -= Initialize;
        }

        private void Initialize(object sender, EventArgs e)
        {
            ReloadPages(true);
        }

        public void ReloadPages(bool force = false)
        {
            if (force) IsLoadingPages = false;
            _ = ReloadPagesAsync();
        }

        public async Task ReloadPagesAsync()
        {
            if (IsLoadingPages) return;
            IsLoadingPages = true;
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            try
            {
                OnLoadingStarted?.Invoke();
                await ReloadPagesAsyncInternal();
            }
            finally
            {
                stopWatch.Stop();
                if (settings.LogPackageSourceTimings)
                    Debug.Log($"PackageSource {name} took {stopWatch.Elapsed.TotalSeconds} seconds to update.");
                IsLoadingPages = false;
                OnLoadingStopped?.Invoke();
            }
        }

        protected abstract Task ReloadPagesAsyncInternal();

        /// <summary>
        /// Generates a new PackageGroup for this PackageSource
        /// </summary>
        /// <param name="author"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="dependencyId">DependencyId for PackageGroup, this is used for mapping dependencies</param>
        /// <param name="tags"></param>
        /// <param name="versions">Collection of version numbers, DependencyIds and dependencies as an array of versioned DependencyIds</param>
        protected void AddPackageGroup(PackageGroupInfo groupInfo)
        {
            if (groupMap == null) groupMap = new Dictionary<string, PackageGroup>();
            if (dependencyMap == null) dependencyMap = new Dictionary<string, HashSet<string>>();
            var group = CreateInstance<PackageGroup>();

            group.Author = groupInfo.Author;
            group.name = group.PackageName = groupInfo.Name;
            group.Description = groupInfo.Description;
            group.DependencyId = groupInfo.DependencyId;
            group.Tags = groupInfo.Tags;
            group.Source = this;
            group.HeaderMarkdown = groupInfo.HeaderMarkdown;
            group.FooterMarkdown = groupInfo.FooterMarkdown;
            groupMap[groupInfo.DependencyId] = group;

            group.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;

            var versionData = groupInfo.Versions.ToArray();
            group.Versions = new PackageVersion[versionData.Length];
            for (int i = 0; i < versionData.Length; i++)
            {
                var versionInfo = versionData[i];
                var versionNumber = versionInfo.Version;
                var versionDependencyId = versionInfo.VersionDependencyId;
                var dependencies = versionInfo.Dependencies;

                var packageVersion = CreateInstance<PackageVersion>();
                packageVersion.name = packageVersion.dependencyId = versionDependencyId;
                packageVersion.group = group;
                packageVersion.version = versionNumber;
                packageVersion.VersionMarkdown = versionInfo.Markdown;
                packageVersion.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
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

        public void LoadPackages()
        {
            Packages?.Clear();
            OnLoadPackages();
            if (Packages.Any())
            {
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
            }
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

        public async Task InstallPackages(IEnumerable<(PackageGroup group, string version)> packages)
        {
            if (EditorApplication.isCompiling) return;
            using (var progressBar = new ProgressBar("Installing Packages"))
            {
                var installSet = new List<PackageVersion>();
                foreach (var (group, version) in packages)
                {
                    var package = group[version];

                    var resolvedVersion = string.Equals(version, "latest", StringComparison.Ordinal) ? package.version : version;

                    installSet.AddRange(EnumerateDependencies(package).Where(dep => !dep.group.Installed));
                }

                var installSetArray = installSet.ToArray();
                var progress = 0.01f;
                var stepSize = 0.33f / installSetArray.Length;

                //Wait till all files are put in place to load new assemblies to make installation more consistent and faster
                try
                {
                    EditorApplication.LockReloadAssemblies();
                    progress = await CreatePackages(installSetArray, progress, stepSize);
                    progress = await CreateManifests(installSetArray, progress, stepSize);
                    progress = await ExtractPackageFiles(installSetArray, progress, stepSize);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    EditorApplication.UnlockReloadAssemblies();
                    EditorUtility.ClearProgressBar();
                    PackageHelper.ResolvePackages();
                }
            }
        }

        public async Task InstallPackage(PackageGroup group, string version)
        {
            if (EditorApplication.isCompiling) return;
            var package = group[version];

            version = string.Equals(version, "latest", StringComparison.Ordinal) ? package.version : version;

            var installSet = EnumerateDependencies(package).Where(dep => !dep.group.Installed).ToArray();
            var progress = 0.01f;
            var stepSize = 0.33f / installSet.Length;

            //Wait till all files are put in place to load new assemblies to make installation more consistent and faster
            try
            {
                using (var progressBar = new ProgressBar("Installing Packages"))
                {
                    EditorApplication.LockReloadAssemblies();
                    progress = await CreatePackages(installSet, progress, stepSize);
                    progress = await CreateManifests(installSet, progress, stepSize);
                    progress = await ExtractPackageFiles(installSet, progress, stepSize);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                progress = 0;
                stepSize = 1;
                progress = await DestroyPackages(installSet, progress, stepSize);
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
                EditorUtility.ClearProgressBar();
                PackageHelper.ResolvePackages();
            }
        }

        private static Task<float> DestroyPackages(PackageVersion[] installSet, float progress, float stepSize)
        {
            using (var progressBar = new ProgressBar("Deleting Packages"))
            {
                progressBar.Update($"{installSet.Length} packages", progress: progress);
                foreach (var installable in installSet)
                {
                    string packageDirectory = installable.group.InstallDirectory;
                    if (Directory.Exists(packageDirectory))
                        Directory.Delete(packageDirectory, true);
                }
            }

            return Task.FromResult(progress);
        }

        private static Task<float> CreatePackages(PackageVersion[] installSet, float progress, float stepSize)
        {
            using (var progressBar = new ProgressBar("Creating Packages"))
            {
                progressBar.Update($"{installSet.Length} packages", progress: progress);
                foreach (var installable in installSet)
                {
                    //This will cause repeated installation of dependencies
                    string packageDirectory = installable.group.InstallDirectory;

                    if (!Directory.Exists(packageDirectory)) Directory.CreateDirectory(packageDirectory);

                    progressBar.Update($"Creating package.json for {installable.group.PackageName}", "Creating Packages", progress += stepSize / 2);
                    PackageHelper.GeneratePackageManifest(
                          installable.group.DependencyId, installable.group.InstallDirectory,
                          installable.group.PackageName, installable.group.Author,
                          installable.version,
                          installable.group.Description);
                }
            }

            return Task.FromResult(progress);
        }

        private static Task<float> CreateManifests(PackageVersion[] installSet, float progress, float stepSize)
        {
            using (var progressBar = new ProgressBar("Creating Package Manifests"))
            {
                progressBar.Update($"Creating {installSet.Length} manifests", progress: progress);
                foreach (var installable in installSet)
                {
                    var installableGroup = installable.group;
                    var manifestGuid = PackageHelper.GetStringHashUTF8(installable.group.DependencyId);
                    var manifestPath = PathExtensions.Combine(installableGroup.InstallDirectory, $"{installableGroup.PackageName}.asset");
                    progressBar.Update($"Creating manifest for {installable.group.PackageName}", progress: progress += stepSize / 3f);
                    var dependenciesArray = installable.dependencies
                                                        .Select(pv => PackageHelper.GetStringHashUTF8(pv.group.DependencyId))
                                                        .Select(guid => $"  - {{fileID: 11400000, guid: {guid}, type: 2}}")
                                                        .ToArray();

                    var manifestYaml = Manifest.GeneratePlainTextManifest(installableGroup.Author, installableGroup.PackageName, installableGroup.Description, installable.version, dependenciesArray);
                    File.WriteAllText(manifestPath, manifestYaml);
                    PackageHelper.WriteAssetMetaData(manifestPath, manifestGuid);
                }
            }

            return Task.FromResult(progress);
        }

        private static Task<float> ExtractPackageFiles(PackageVersion[] installSet, float progress, float stepSize)
        {
            using (var progressBar = new ProgressBar("Installing Package Files"))
            {
                progressBar.Update($"{installSet.Length} packages", progress: progress);
                foreach (var installable in installSet)
                {
                    string packageDirectory = installable.group.InstallDirectory;

                    progressBar.Update($"Downloading {installable.group.PackageName}", progress: progress += stepSize / 2);

                    installable.group.Source.OnInstallPackageFiles(installable, packageDirectory);

                    foreach (var assemblyPath in Directory.GetFiles(packageDirectory, "*.dll", SearchOption.AllDirectories))
                        PackageHelper.WriteAssemblyMetaData(assemblyPath, $"{assemblyPath}.meta");

                }
            }

            return Task.FromResult(progress);
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [Serializable]
        public class PackageVersionInfo
        {
            public string version;
            public string versionDependencyId;
            public string[] dependencies;
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
                        if (!sourceGroups.ContainsKey(packageSource.SourceGroup))
                            sourceGroups[packageSource.SourceGroup] = new List<PackageSource>();

                        if (!sourceGroups[packageSource.SourceGroup].Contains(packageSource))
                            sourceGroups[packageSource.SourceGroup].Add(packageSource);
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

                if (!dependencyMap.ContainsKey(packageVersion.dependencyId))
                    dependencyMap[packageVersion.dependencyId] = new HashSet<string>();

                foreach (var depDepId in dependencies)
                    dependencyMap[packageVersion.dependencyId].Add(depDepId);
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
            OnLoadPackages();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var versionMap = Packages.Where(pkgGrp => pkgGrp?.Versions != null).SelectMany(pkgGrp => pkgGrp.Versions.Select(pkgVer => new KeyValuePair<PackageGroup, PackageVersion>(pkgGrp, pkgVer))).ToDictionary(ver => ver.Value.dependencyId);

            foreach (var packageGroup in Packages)
            {
                var groupName = packageGroup.PackageName;
                foreach (var version in packageGroup.Versions)
                {
                    var dependencies = dependencyMap[version.name].ToArray();
                    version.dependencies = new PackageVersion[dependencies.Length];
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        string dependencyId = dependencies[i];
                        string groupId = VersionIdToGroupId(dependencyId);
                        if (versionMap.ContainsKey(dependencyId))
                        {
                            version.dependencies[i] = versionMap[dependencyId].Value;
                        }
                        else if (groupMap.ContainsKey(groupId))
                        {
                            version.dependencies[i] = groupMap[groupId]["latest"];
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

        public async Task InstallPackage(PackageGroup group, string version)
        {
            var package = group[version];

            var installSet = EnumerateDependencies(package).Where(dep => !dep.group.Installed).ToArray();

            foreach (var installable in installSet)
            {
                //This will cause repeated installation of dependencies
                if (Directory.Exists(installable.group.PackageDirectory)) Directory.Delete(installable.group.PackageDirectory, true);
                Directory
                    .CreateDirectory(installable.group.PackageDirectory);

                await installable.group.Source.OnInstallPackageFiles(installable, installable.group.PackageDirectory);

                foreach (var assemblyPath in Directory.GetFiles(installable.group.PackageDirectory, "*.dll", SearchOption.AllDirectories))
                    PackageHelper.WriteAssemblyMetaData(assemblyPath, $"{assemblyPath}.meta");
            }
            var tempRoot = Path.Combine(Path.Combine("Assets", "ThunderKitSettings"), "Temp");
            Directory.CreateDirectory(tempRoot);
            foreach (var installableForManifestCreation in installSet)
            {
                var installableGroup = installableForManifestCreation.group;
                var assetTempPath = Path.Combine(tempRoot, $"{installableGroup.PackageName}.asset");
                if (AssetDatabase.LoadAssetAtPath<Manifest>(assetTempPath))
                    AssetDatabase.DeleteAsset(assetTempPath);

                var identity = CreateInstance<ManifestIdentity>();
                identity.name = nameof(ManifestIdentity);
                identity.Author = installableGroup.Author;
                identity.Description = installableGroup.Description;
                identity.Name = installableGroup.PackageName;
                identity.Version = version;
                var manifest = ScriptableHelper.EnsureAsset<Manifest>(assetTempPath);
                manifest.InsertElement(identity, 0);
                manifest.Identity = identity;
                PackageHelper.WriteAssetMetaData(assetTempPath, $"{assetTempPath}.meta");
            }
            foreach (var installableForDependencies in installSet)
            {
                var manifestAssetPath = Path.Combine(tempRoot, $"{installableForDependencies.group.PackageName}.asset");
                var allReps = AssetDatabase.LoadAllAssetsAtPath(manifestAssetPath);
                var allIdents = allReps.OfType<ManifestIdentity>();
                var identity = allIdents.First();
                identity.Dependencies = new Manifest[installableForDependencies.dependencies.Length];
                for (int i = 0; i < installableForDependencies.dependencies.Length; i++)
                {
                    var installableDependency = installableForDependencies.dependencies[i];
                    var dependencyAssetTempPath = Path.Combine(tempRoot, $"{installableDependency.group.PackageName}.asset");
                    var manifest = AssetDatabase.LoadAssetAtPath<Manifest>(dependencyAssetTempPath);
                    if (!manifest)
                    {
                        var dependencyAssetPackagePath = Path.Combine(Path.Combine("Packages", installableDependency.dependencyId), $"{installableDependency.group.PackageName}.asset");
                        manifest = AssetDatabase.LoadAssetAtPath<Manifest>(dependencyAssetPackagePath);
                    }
                    if (!manifest)
                    {
                        string[] manifests = AssetDatabase.FindAssets($"t:{nameof(Manifest)} {installableDependency.group.PackageName}",
                                                              new string[] { "Assets", "Packages" });

                        manifest = manifests.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Manifest>).First();
                    }
                    identity.Dependencies[i] = manifest;
                }
            }
            foreach (var installableForManifestMove in installSet)
            {
                var assetTempPath = Path.Combine(tempRoot, $"{installableForManifestMove.group.PackageName}.asset");
                var assetMetaTempPath = Path.Combine(tempRoot, $"{installableForManifestMove.group.PackageName}.asset.meta");
                var assetPackagePath = Path.Combine(installableForManifestMove.group.PackageDirectory, $"{installableForManifestMove.group.PackageName}.asset");
                var assetMetaPackagePath = Path.Combine(installableForManifestMove.group.PackageDirectory, $"{installableForManifestMove.group.PackageName}.asset.meta");

                var fileData = File.ReadAllText(assetTempPath);

                AssetDatabase.DeleteAsset(assetTempPath);

                File.WriteAllText(assetPackagePath, fileData);
                PackageHelper.WriteAssetMetaData(assetPackagePath, $"{assetPackagePath}.meta");
            }
            foreach (var installable in installSet)
                PackageHelper.GeneratePackageManifest(
                    installable.group.DependencyId.ToLower(), installable.group.PackageDirectory,
                    installable.group.PackageName, installable.group.Author,
                    installable.version,
                    installable.group.Description);

            Directory.Delete(tempRoot);
            File.Delete($"{tempRoot}.meta");

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Executes the downloading, unpacking, and placing of package files.  Files
        /// </summary>
        /// <param name="version">The version of the Package which should be installed</param>
        /// <param name="packageDirectory">Root directory which files should be extracted into</param>
        /// <returns></returns>
        public abstract Task OnInstallPackageFiles(PackageVersion version, string packageDirectory);


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
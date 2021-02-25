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
        protected void AddPackageGroup(string author, string name, string description, string dependencyId, string[] tags, Func<IEnumerable<(string version, string dependencyId, string[] dependencies)>> getVersionData)
        {
            if (dependencyMap == null) dependencyMap = new Dictionary<string, HashSet<string>>();
            if (Packages == null) Packages = new List<PackageGroup>();
            var group = CreateInstance<PackageGroup>();

            group.Author = author;
            group.name = group.PackageName = name;
            group.Description = description;
            group.DependencyId = dependencyId;
            group.Tags = tags;
            group.Source = this;

            group.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
            AssetDatabase.AddObjectToAsset(group, this);

            var versionData = getVersionData().ToArray();
            group.Versions = new PackageVersion[versionData.Length];
            for (int i = 0; i < versionData.Length; i++)
            {
                var (version, versionDependencyId, dependencies) = versionData[i];

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
        protected abstract void LoadPackagesInternal();

        public void LoadPackages()
        {
            LoadPackagesInternal();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var allVersions = Packages.Where(pkgGrp => pkgGrp?.Versions != null).SelectMany(pkgGrp => pkgGrp.Versions).ToArray();
            var versionMap = allVersions.ToDictionary(ver => ver.dependencyId);
            foreach (var packageGroup in Packages)
            {
                foreach (var version in packageGroup.Versions)
                {
                    var dependencies = dependencyMap[version.dependencyId].ToArray();
                    version.dependencies = new PackageVersion[dependencies.Length];
                    for (int i = 0; i < dependencies.Length; i++)
                        if (versionMap.ContainsKey(dependencies[i]))
                            version.dependencies[i] = versionMap[dependencies[i]];
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        public async Task InstallPackage(PackageGroup group, string version)
        {
            if (Directory.Exists(group.PackageDirectory)) Directory.Delete(group.PackageDirectory);

            Directory.CreateDirectory(group.PackageDirectory);

            var package = group[version];

            if (SourceGroups.ContainsKey(SourceGroup))
            {
                var sourceGroup = SourceGroups[SourceGroup];
                var sourcePackages = sourceGroup.ToDictionary(source => source, source => source.Packages);

                var inverseDependencyMap = new Dictionary<PackageGroup, List<PackageGroup>>();
                //nothing depends on the package being installed by this function, however keys will be used to determine when packages will be installed by the content of their values
                inverseDependencyMap[group] = new List<PackageGroup>(0);

                //var pendingDependencies = new HashSet<string>(group[version].dependencies);

                //var finalDependencies = new List<(PackageGroup, PackageVersion)>();

                //var currentDependant = group;
                //while (pendingDependencies.Any())
                //{
                //    foreach (var source in sourceGroup)
                //    {
                //        var packages = sourcePackages[source];
                //        var dependencies = packages.Where(pkg => pkg.VersionIds.Any(pendingDependencies.Contains));

                //        foreach (var dep in dependencies)
                //        {
                //            if (!inverseDependencyMap.ContainsKey(dep)) inverseDependencyMap[dep] = new List<PackageGroup>();
                //            var depVersion = dep.Versions.First(pv => pendingDependencies.Contains(pv.dependencyId));
                //            pendingDependencies.Remove(depVersion.dependencyId);



                //            //inverseDependencyMap[dep]
                //        }

                //        //foreach (var (packageGroup, packageVersions) in dependencies)
                //        //    foreach (var packageVersion in packageVersions)
                //        //    {
                //        //        pendingDependencies.Remove(packageVersion.dependencyId);
                //        //        if (finalDependencies.Contains((packageGroup, packageVersion))) continue;
                //        //        finalDependencies.Add((packageGroup, packageVersion));

                //        //        foreach (var nestedDependency in packageVersion.dependencies)
                //        //            pendingDependencies.Add(nestedDependency);
                //        //    }
                //    }
                //}

                //var builder = new StringBuilder();
                //builder.AppendLine($"Found {finalDependencies.Count} dependencies");
                //foreach (var (pg, pv) in finalDependencies)
                //    builder.AppendLine(pv.dependencyId);

                //Debug.Log(builder.ToString());

                //foreach (var (pg, pv) in finalDependencies)
                //{
                //    //This will cause repeated installation of dependencies
                //    if (Directory.Exists(pg.PackageDirectory)) Directory.Delete(pg.PackageDirectory);
                //    Directory
                //        .CreateDirectory(pg.PackageDirectory);

                //    await pg.Source.InstallPackageFiles(pg, pg[version], pg.PackageDirectory);

                //    EstablishPackage(pg, version);

                //}
            }

            await InstallPackageFiles(group, group[version], group.PackageDirectory);

            EstablishPackage(group, version);

            AssetDatabase.Refresh();
        }

        private static void EstablishPackage(PackageGroup pg, string version)
        {
            var identity = CreateInstance<ManifestIdentity>();
            identity.Author = pg.Author;
            identity.Description = pg.Description;
            identity.Name = pg.PackageName;
            identity.Version = version;

            var assetTempPath = Path.Combine("Assets", $"{pg.PackageName}.asset");
            var assetMetaTempPath = Path.Combine("Assets", $"{pg.PackageName}.asset.meta");
            var assetFinalPath = Path.Combine(pg.PackageDirectory, $"{pg.PackageName}.asset");
            var assetMetaFinalPath = Path.Combine(pg.PackageDirectory, $"{pg.PackageName}.asset.meta");
            var manifest = ScriptableHelper.EnsureAsset<Manifest>(assetTempPath);
            manifest.InsertElement(identity, 0);

            var fileData = File.ReadAllText(assetTempPath);
            var metafileData = File.ReadAllText(assetMetaTempPath);
            AssetDatabase.DeleteAsset(assetTempPath);

            File.WriteAllText(assetFinalPath, fileData);
            File.WriteAllText(assetMetaFinalPath, metafileData);

            PackageHelper.GeneratePackageManifest(
                pg[version].dependencyId.ToLower(), pg.PackageDirectory,
                pg.PackageName, pg.Author,
                pg[version].version,
                pg.Description);
        }

        //Have this return a set of files for InstallPackage to consume for Manifest construction
        public abstract Task InstallPackageFiles(PackageGroup package, PackageVersion version, string packageDirectory);


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
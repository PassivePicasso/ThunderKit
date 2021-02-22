using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Common.Package;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.PackageManager
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

        public abstract string Name { get; }
        public abstract string SourceGroup { get; }

        public async Task InstallPackage(PackageGroup group, string version)
        {
            if (Directory.Exists(group.PackageDirectory)) Directory.Delete(group.PackageDirectory);

            Directory.CreateDirectory(group.PackageDirectory);

            var package = group[version];

            if (SourceGroups.ContainsKey(SourceGroup))
            {
                var sourceGroup = SourceGroups[SourceGroup];
                var sourcePackages = sourceGroup.ToDictionary(source => source, source => (IEnumerable<PackageGroup>)source.GetPackages().ToArray());

                var pendingDependencies = new HashSet<string>(package.dependencies);
                var finalDependencies = new List<(PackageGroup, PackageVersion)>();
                while (pendingDependencies.Any())
                {
                    foreach (var source in sourceGroup)
                    {
                        var packages = sourcePackages[source];
                        var dependencies = packages
                            .Select(pkgGrp =>
                                (pkgGrp,
                                 pkgGrp.versions.Where(pkgVerson => pendingDependencies.Contains(pkgVerson.dependencyId))
                                )
                            );
                        foreach (var (packageGroup, packageVersions) in dependencies)
                            foreach (var packageVersion in packageVersions)
                            {
                                pendingDependencies.Remove(packageVersion.dependencyId);
                                if (finalDependencies.Contains((packageGroup, packageVersion))) continue;
                                finalDependencies.Add((packageGroup, packageVersion));

                                foreach (var nestedDependency in packageVersion.dependencies)
                                    pendingDependencies.Add(nestedDependency);
                            }
                    }
                }
                var builder = new StringBuilder();
                builder.AppendLine($"Found {finalDependencies.Count} dependencies");
                foreach (var (pg, pv) in finalDependencies)
                    builder.AppendLine(pv.dependencyId);
                Debug.Log(builder.ToString());

                foreach (var (pg, pv) in finalDependencies)
                {
                    if (Directory.Exists(pg.PackageDirectory))
                        Directory.Delete(pg.PackageDirectory);
                    Directory
                        .CreateDirectory(pg.PackageDirectory);

                    await pg.Source.InstallPackageFiles(pg, pv, pg.PackageDirectory);

                    PackageHelper.GeneratePackageManifest(
                        pv.dependencyId.ToLower(), pg.PackageDirectory,
                        pg.name, pg.author,
                        pv.version,
                        pg.description,
                        pg.package_url);
                }
            }

            await InstallPackageFiles(group, package, group.PackageDirectory);

            PackageHelper.GeneratePackageManifest(
                package.dependencyId.ToLower(), group.PackageDirectory,
                group.name, group.author,
                package.version,
                group.description,
                group.package_url);

            AssetDatabase.Refresh();
        }

        public abstract Task InstallPackageFiles(PackageGroup package, PackageVersion version, string packageDirectory);
        public IEnumerable<PackageGroup> GetPackages(string filter = "")
        {
            foreach (var package in GetPackagesInternal(filter))
            {
                package.Source = this;

                yield return package;
            }
        }

        protected abstract IEnumerable<PackageGroup> GetPackagesInternal(string filter = "");

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
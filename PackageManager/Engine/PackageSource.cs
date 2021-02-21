using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ThunderKit.Common.Package;
using ThunderKit.PackageManager.Engine;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.PackageManager.Model
{
    public abstract class PackageSource : ScriptableObject
    {
        public async void InstallPackage(PackageGroup group, string version, string packageDirectory)
        {
            if (Directory.Exists(packageDirectory)) Directory.Delete(packageDirectory);
            
            Directory.CreateDirectory(packageDirectory);

            var package = group[version];

            await InstallPackageFiles(group, package, packageDirectory);

            PackageHelper.GeneratePackageManifest(
                package.dependencyId.ToLower(), packageDirectory,
                group.name, GetName(),
                package.version,
                group.description,
                group.package_url);

            AssetDatabase.Refresh();
        }
        protected abstract Task InstallPackageFiles(PackageGroup package, PackageVersion version, string packageDirectory);
        public IEnumerable<PackageGroup> GetPackages(string filter = "")
        {
            foreach (var package in GetPackagesInternal(filter))
            {
                package.Source = this;
                yield return package;
            }
        }

        protected abstract IEnumerable<PackageGroup> GetPackagesInternal(string filter = "");
        public abstract string GetName();
    }
}
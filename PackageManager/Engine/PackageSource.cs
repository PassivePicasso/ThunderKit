using System.Collections.Generic;
using UnityEngine;

namespace ThunderKit.PackageManager.Model
{
    public abstract class PackageSource : ScriptableObject
    {
        public abstract void InstallPackage(PackageGroup package, string version, string packageDirectory);
        public abstract IEnumerable<PackageGroup> GetPackages(string filter = "");
        public abstract string GetName();
    }
}
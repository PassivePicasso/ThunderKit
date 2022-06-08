using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Config
{
    [System.Serializable]
    public class TkPackageManifest 
    {
        [System.Serializable]
        public struct Dependency : System.IEquatable<Dependency>
        {
            public string DependencyId;
            public string Version;

            public override bool Equals(object obj)
            {
                return obj is Dependency dependency && Equals(dependency);
            }

            public bool Equals(Dependency other)
            {
                return DependencyId == other.DependencyId &&
                       Version == other.Version;
            }

            public override int GetHashCode()
            {
                int hashCode = -29895018;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DependencyId);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Version);
                return hashCode;
            }

            public static bool operator ==(Dependency left, Dependency right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Dependency left, Dependency right)
            {
                return !(left == right);
            }
        }

        internal static TkPackageManifest instance;
        static readonly string ManifestPath = $"ProjectSettings/ThunderKit/{nameof(TkPackageManifest)}.json";

        //[InitializeOnLoadMethod]
        public static async Task Initialize()
        {
            var changed = false;

            Directory.CreateDirectory(Path.GetDirectoryName(ManifestPath));

            if (instance == null && File.Exists(ManifestPath))
            {
                var json = File.ReadAllText(ManifestPath);
                instance = JsonUtility.FromJson<TkPackageManifest>(json);
            }
            else if (instance == null)
            {
                instance = new TkPackageManifest();

                foreach (var source in PackageSourceSettings.PackageSources)
                    foreach (var targetPackage in source.Packages)
                    {
                        var dep = instance.Dependencies.FirstOrDefault(d => d.DependencyId == targetPackage.DependencyId);
                        if (targetPackage.Installed && dep == default)
                            instance.Dependencies.Add(new Dependency { DependencyId = targetPackage.DependencyId, Version = targetPackage.InstalledVersion });
                    }

                changed = true;
            }

            foreach (var packageSource in PackageSourceSettings.PackageSources)
                foreach (var pkg in packageSource.Packages)
                {
                    var existingDep = instance.Dependencies.FirstOrDefault(dp => dp.DependencyId == pkg.DependencyId);
                    if (pkg.Installed && existingDep == default)
                    {
                        instance.Dependencies.Add(new Dependency { DependencyId = pkg.DependencyId, Version = pkg.InstalledVersion });
                        changed = true;
                    }
                }

            if(changed) Write();

            var dependencies = instance.Dependencies;
            foreach (var pkgSource in PackageSourceSettings.PackageSources)
                foreach (var potentialPkg in pkgSource.Packages)
                {
                    var dep = dependencies.FirstOrDefault(d => d.DependencyId == potentialPkg.DependencyId);
                    if (dep != default && !potentialPkg.Installed)
                    {
                        dependencies.Remove(dep);
                        await pkgSource.InstallPackage(potentialPkg, dep.Version);
                    }
                }
        }

        private static void Write()
        {
            var json = JsonUtility.ToJson(instance);
            File.WriteAllText(ManifestPath, json);
        }

        public List<Dependency> Dependencies;

        public TkPackageManifest()
        {
            Dependencies = new List<Dependency>();
        }
    }
}
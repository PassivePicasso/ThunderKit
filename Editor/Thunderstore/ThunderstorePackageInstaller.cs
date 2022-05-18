using System;
using System.Linq;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore
{
    public abstract class ThunderstorePackageInstaller : OptionalExecutor
    {
        private const string transientStoreName = "transient-store";

        public abstract string DependencyId { get; }
        public abstract string ThunderstoreAddress { get; }

        private ThunderstoreSource transientStore;
        public sealed override bool Execute()
        {
            try
            {
                EditorApplication.LockReloadAssemblies();
                var packageSource = PackageSourceSettings.PackageSources.OfType<ThunderstoreSource>().FirstOrDefault(source => source.Url == ThunderstoreAddress);
                if (!packageSource)
                {
                    if (transientStore)
                        packageSource = transientStore;
                    else
                    {
                        packageSource = CreateInstance<ThunderstoreSource>();
                        packageSource.Url = ThunderstoreAddress;
                        packageSource.name = transientStoreName;
                        packageSource.ReloadPages();
                        transientStore = packageSource;
                    }
                }
                else if (packageSource.Packages == null || packageSource.Packages.Count == 0)
                {
                    packageSource.ReloadPages();
                }

                if (packageSource.Packages == null || packageSource.Packages.Count == 0)
                {
                    Debug.LogWarning($"PackageSource at \"{ThunderstoreAddress}\" has no packages");
                    return false;
                }

                var package = packageSource.Packages.FirstOrDefault(pkg => pkg.DependencyId == DependencyId);
                if (package == null)
                {
                    Debug.LogWarning($"Could not find package with DependencyId of \"{DependencyId}\"");
                    return false;
                }

                if (package.Installed)
                {
                    Debug.LogWarning($"Not installing package with DependencyId of \"{DependencyId}\" because it's already installed");
                    return true;
                }

                Debug.Log($"Installing latest version of package \"{DependencyId}\"");
                var task = packageSource.InstallPackage(package, "latest");
                while (!task.IsCompleted)
                {
                    Debug.Log("Waiting for completion");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return false;
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }

            PackageHelper.ResolvePackages();

            return true;
        }

        public override void Cleanup()
        {
            if (transientStore)
                DestroyImmediate(transientStore);
        }
    }
}


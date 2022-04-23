using System;
using System.Linq;
using ThunderKit.Core.Config;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore
{
    public abstract class ThunderstorePackageInstaller : OptionalExecutor
    {
        public abstract string DependencyId { get; }
        public abstract string SourcePath { get; }

        public sealed override void Execute()
        {
            try
            {
                EditorApplication.LockReloadAssemblies();
                var packageSource = AssetDatabase.LoadAssetAtPath<ThunderstoreSource>(SourcePath);
                var package = packageSource.Packages.FirstOrDefault(pkg => pkg.DependencyId == DependencyId);
                if (package == null)
                {
                    Debug.LogWarning($"Could not find package with DependencyId of {DependencyId}");
                    return;
                }

                if (package.Installed)
                {
                    Debug.LogWarning($"Not installing package with DependencyId of {DependencyId} because it's already installed");
                    return;
                }

                Debug.Log($"Installing latest version of package {DependencyId});");
                var task = packageSource.InstallPackage(package, "latest");
                while (!task.IsCompleted)
                {
                    Debug.Log("Waiting for completion");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
                PackageHelper.ResolvePackages();
            }
        }
    }
}


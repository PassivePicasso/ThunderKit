using ThunderKit.Core.Utilities;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ThunderKit.Core.Config.Common
{
    public abstract class UnityPackageUninstaller : OptionalExecutor
    {
        public abstract string PackageIdentifier { get; }

        public sealed override bool Execute()
        {
            Request result = Client.Remove(PackageIdentifier);
            var escape = false;
            while (!result.IsCompleted && !escape)
            {
                var x = escape; //Break and set in case of unexpected infinite loop
            }
            Debug.Log($"Uninstalled {PackageIdentifier}");

            PackageHelper.ResolvePackages();
            return true;
        }
    }
}

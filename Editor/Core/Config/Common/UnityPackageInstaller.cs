using ThunderKit.Core.Utilities;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ThunderKit.Core.Config.Common
{
    public abstract class UnityPackageInstaller : OptionalExecutor
    {
        public abstract string PackageIdentifier { get; }

        public sealed override void Execute()
        {
            Request result = Client.Add(PackageIdentifier);
            var escape = false;
            while (!result.IsCompleted && !escape)
            {
                var x = escape; //Break and set in case of unexpected infinite loop
            }
            Debug.Log($"Installed {PackageIdentifier}");

            PackageHelper.ResolvePackages();
        }
    }
}

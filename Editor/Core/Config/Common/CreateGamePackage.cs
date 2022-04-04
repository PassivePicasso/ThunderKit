using System.IO;
using System.Threading.Tasks;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Config
{
    public class CreateGamePackage : Executor
    {
        public override int Priority => Constants.GameConfigurationPriority.CreateGamePackage;

        public override async Task Execute()
        {
            while (EditorApplication.isUpdating)
                await Task.Yield();

            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var packageName = Path.GetFileNameWithoutExtension(settings.GameExecutable);
            PackageHelper.GeneratePackageManifest(settings.PackageName, settings.PackageFilePath, packageName, PlayerSettings.companyName, Application.version, $"Imported assemblies from game {packageName}");
        }
    }
}
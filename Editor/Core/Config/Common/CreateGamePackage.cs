using System;
using System.IO;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Config
{
    public class CreateGamePackage : OptionalExecutor
    {
        public override int Priority => Constants.ConfigPriority.CreateGamePackage;

        public override void Execute()
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var packageName = Path.GetFileNameWithoutExtension(settings.GameExecutable);
            PackageHelper.GeneratePackageManifest(settings.PackageName,
                settings.PackageFilePath, packageName,
                PlayerSettings.companyName,
                Application.version,
                $"Imported assemblies from game {packageName}");

            var fullPackagePath = Path.GetFullPath(settings.PackageFilePath);
            var files = Directory.EnumerateFiles(fullPackagePath);
            foreach(var file in files)
            {
                File.SetLastWriteTime(file, DateTime.Now);
            }

            AssetDatabase.ImportAsset(fullPackagePath, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        }
    }
}
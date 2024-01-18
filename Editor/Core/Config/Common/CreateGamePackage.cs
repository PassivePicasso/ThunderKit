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
        public override int Priority => Constants.Priority.CreateGamePackage;

        public override string Description => "Creates the package.json file for the imported game so it can be recognized and loaded by the Unity PackageManager.";

        public override bool Execute()
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var packageName = Path.GetFileNameWithoutExtension(settings.GameExecutable);

            Directory.CreateDirectory(settings.PackageFilePath);

            ThunderKit.Common.Configuration.ScriptingSymbolManager.AddScriptingDefine(settings.PackageName);
            PackageHelper.GeneratePackageManifest(settings.PackageName,
                settings.PackageFilePath, packageName,
                PlayerSettings.companyName,
                Application.version,
                $"Imported assemblies from game {packageName}");

            var fullPackagePath = Path.GetFullPath(settings.PackageFilePath);
            var files = Directory.EnumerateFiles(fullPackagePath);
            foreach (var file in files)
            {
                try
                {
                    File.SetLastWriteTime(file, DateTime.Now);
                    File.SetCreationTime(file, DateTime.Now);
                    File.SetLastAccessTime(file, DateTime.Now);
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }
            }

            PackageHelper.ResolvePackages();
            return true;
        }
    }
}
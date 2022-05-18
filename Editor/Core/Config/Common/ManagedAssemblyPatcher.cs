using System.IO;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEditor;


namespace ThunderKit.Core.Config
{
    public abstract class ManagedAssemblyPatcher : OptionalExecutor
    {
        public abstract string AssemblyFileName { get; }
        public abstract string BsDiffPatchPath { get; }

        public override bool Execute()
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var originalAssembly = Path.GetFullPath(Path.Combine(settings.ManagedAssembliesPath, AssemblyFileName));
            var destinationAssembly = Path.GetFullPath(Path.Combine(settings.PackageFilePath, AssemblyFileName));
            var diffPath = Path.GetFullPath(BsDiffPatchPath);
            var packagePath = Path.GetDirectoryName(destinationAssembly);

            if (File.Exists(destinationAssembly))
                File.Delete(destinationAssembly);

            Directory.CreateDirectory(packagePath);

            BsDiff.BsTool.Patch(originalAssembly, destinationAssembly, diffPath);

            var asmPath = destinationAssembly.Replace("\\", "/");
            var destinationMetaData = Path.Combine(settings.PackageFilePath, $"{AssemblyFileName}.meta");
            PackageHelper.WriteAssemblyMetaData(asmPath, destinationMetaData);

            var escape = false;
            while (EditorApplication.isUpdating && !escape)
            {
                var x = escape;
            }
            return true;
        }
    }
}
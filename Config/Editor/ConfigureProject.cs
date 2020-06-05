#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Utilities;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.AutoConfig.Editor
{
    public class ConfigureProject
    {
        [MenuItem("ThunderKit/Configure ThunderKit")]
        private static void ValidateReferences()
        {
            string currentDir = Directory.GetCurrentDirectory();
            var settings = ThunderKitSettings.GetOrCreateSettings();

            if (string.IsNullOrEmpty(settings.GameExecutable))
            {
                string executablePath = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "exe");
                settings.GameExecutable = Path.GetFileName(executablePath);
                settings.GamePath = Path.GetDirectoryName(executablePath);
                EditorUtility.SetDirty(settings);
            }
            else
            {
                var foundExecutable = string.IsNullOrEmpty(settings.GamePath)
                                    ? false
                                    : Directory.EnumerateFiles(settings.GamePath ?? currentDir, settings.GameExecutable).Any();

                while (!foundExecutable)
                {
                    string path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "exe");
                    if (string.IsNullOrEmpty(path)) return;
                    settings.GamePath = Path.GetDirectoryName(path);
                    foundExecutable = Directory.EnumerateFiles(settings.GamePath, settings.GameExecutable).Any();
                }
                EditorUtility.SetDirty(settings);
            }

            if (string.IsNullOrEmpty(settings.GamePath) || string.IsNullOrEmpty(settings.GameExecutable)) return;

            var destinationFolder = Path.Combine(currentDir, "Assets", "Assemblies");
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            Debug.Log("Acquiring references");

            EditorUtility.SetDirty(settings);

            var locations = AppDomain.CurrentDomain.GetAssemblies().Where(asm => !asm.IsDynamic).Select(asm => asm.Location).ToArray();
            var managedPath = Path.Combine(settings.GamePath, $"{Path.GetFileNameWithoutExtension(settings.GameExecutable)}_Data", "Managed");
            foreach (var asm in Directory.EnumerateFiles(managedPath, "*.dll"))
            {
                if (locations.Any(l => l.Contains(Path.GetFileNameWithoutExtension(asm)))) continue;

                var destinationFile = Path.Combine(destinationFolder, Path.GetFileName(asm));

                var destinationMetaData = Path.Combine(currentDir, "Assets", "Assemblies", $"{asm}.meta");

                if (File.Exists(destinationFile)) File.Delete(destinationFile);
                File.Copy(asm, destinationFile);

                File.WriteAllText(destinationMetaData, MetaData);
            }

            _ = BepInExPackLoader.DownloadBepinex();

            AssetDatabase.Refresh();
        }

        internal const string MetaData =
@"fileFormatVersion: 2
guid: fc64261ca6282254da01b0f016bcfcea
PluginImporter:
  externalObjects: {}
  serializedVersion: 2
  iconMap: {}
  executionOrder: {}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 0
  isExplicitlyReferenced: 1
  validateReferences: 1
  platformData:
  - first:
      '': Any
    second:
      enabled: 0
      settings:
        Exclude Editor: 0
        Exclude Linux: 0
        Exclude Linux64: 0
        Exclude LinuxUniversal: 0
        Exclude OSXUniversal: 0
        Exclude Win: 0
        Exclude Win64: 0
  - first:
      Any: 
    second:
      enabled: 1
      settings: {}
  - first:
      Editor: Editor
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
        DefaultValueInitialized: true
        OS: AnyOS
  - first:
      Facebook: Win
    second:
      enabled: 0
      settings:
        CPU: AnyCPU
  - first:
      Facebook: Win64
    second:
      enabled: 0
      settings:
        CPU: AnyCPU
  - first:
      Standalone: Linux
    second:
      enabled: 1
      settings:
        CPU: x86
  - first:
      Standalone: Linux64
    second:
      enabled: 1
      settings:
        CPU: x86_64
  - first:
      Standalone: LinuxUniversal
    second:
      enabled: 1
      settings: {}
  - first:
      Standalone: OSXUniversal
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
  - first:
      Standalone: Win
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
  - first:
      Standalone: Win64
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
  - first:
      Windows Store Apps: WindowsStoreApps
    second:
      enabled: 0
      settings:
        CPU: AnyCPU
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";
    }
}
#endif
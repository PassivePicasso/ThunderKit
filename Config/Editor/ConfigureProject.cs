#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Utilities;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.AutoConfig.Editor
{
    [InitializeOnLoad]
    public class ConfigureProject
    {
        static ConfigureProject()
        {
            EditorApplication.update += ValidateReferences;
            AssemblyReloadEvents.beforeAssemblyReload += ValidateReferences;
        }

        private static void ValidateReferences()
        {
            string currentDir = Directory.GetCurrentDirectory();
            var settings = ThunderKitSettings.GetOrCreateSettings();

            var results = settings.RequiredAssemblies.SelectMany(requiredAssembly => AssetDatabase.FindAssets(requiredAssembly)).Distinct().ToArray();
            var fileResults = results.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).Where(p => p.StartsWith("Assets/Assemblies")).ToList();
            var destinationFolder = Path.Combine(currentDir, "Assets", "Assemblies");

            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            if (!settings.RequiredAssemblies.Any() || settings.RequiredAssemblies.All(asm => File.Exists(Path.Combine(destinationFolder, asm)))
             || (settings.RequiredAssemblies.All(ra => fileResults.Any(r => r.Contains(ra)))
                 && !string.IsNullOrEmpty(settings.GamePath)
                 && File.Exists(Path.Combine(settings.GamePath, settings.GameExecutable))))
                return;

            Debug.Log("Acquiring references");

            if (string.IsNullOrEmpty(settings.GameExecutable))
            {
                string executablePath = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "exe");
                settings.GameExecutable = Path.GetFileName(executablePath);
                settings.GamePath = Path.GetFileName(Path.GetDirectoryName(executablePath));
                settings.SetDirty();
            }
            else
            {
                while (!Directory.EnumerateFiles(settings.GamePath, settings.GameExecutable).Any())
                    settings.GamePath = Path.GetDirectoryName(EditorUtility.OpenFilePanel("Open Game Executable", currentDir, settings.GameExecutable));

            }

            settings.SetDirty();

            foreach (var asm in settings.RequiredAssemblies)
            {
                var destinationFile = Path.Combine(currentDir, "Assets", "Assemblies", $"{asm}.dll");
                var assemblyPath = Path.Combine(settings.GamePath, $"{Path.GetFileNameWithoutExtension(settings.GameExecutable)}_Data", "Managed", $"{asm}.dll");
                var destinationMetaData = Path.Combine(currentDir, "Assets", "Assemblies", $"{asm}.meta");

                if (File.Exists(destinationFile)) File.Delete(destinationFile);
                File.Copy(assemblyPath, destinationFile);

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
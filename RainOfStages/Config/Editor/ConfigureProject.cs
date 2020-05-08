#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RainOfStages.AutoConfig
{
    [InitializeOnLoad]
    public class ConfigureProject
    {

        private static string[] RequiredAssemblies = new[]
        {
            "Assembly-CSharp",
            "Facepunch.Steamworks",
            "Rewired_Core",
            "Rewired_CSharp",
            "Rewired_Windows_Lib",
            "Unity.Postprocessing.Runtime",
            "WWise",
            "Zio"
        };
        private const string RoR2Executable = "Risk of Rain 2.exe";

        static ConfigureProject()
        {
            EditorApplication.update += ValidateRoRReferences;
            AssemblyReloadEvents.beforeAssemblyReload += ValidateRoRReferences;
        }

        [MenuItem("Assets/Rain of Stages/Setup DnSpy")]
        public static void LocateDnSpy()
        {
            var settings = RainOfStagesSettings.GetOrCreateSettings();
            settings.DnSpyPath = EditorUtility.OpenFolderPanel("Open dnSpy Root Folder", Directory.GetCurrentDirectory(), "dnSpy.exe");
            EditorUtility.SetDirty(settings);
        }

        private static void ValidateRoRReferences()
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            var settings = RainOfStagesSettings.GetOrCreateSettings();
            var results = RequiredAssemblies.SelectMany(ra => AssetDatabase.FindAssets(ra)).Distinct().ToArray();
            var destinationFolder = Path.Combine(projectDirectory, "Assets", "Assemblies");

            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            if (RequiredAssemblies.All(asm => File.Exists(Path.Combine(destinationFolder, asm))))
                return;

            if (results.Length == RequiredAssemblies.Length
             && !string.IsNullOrEmpty(settings.RoR2Path)
             && File.Exists(Path.Combine(settings.RoR2Path, "Risk of Rain 2.exe")))
                return;

            Debug.Log("Acquiring references for Rain of Stages");

            string ror2Path = projectDirectory;
            while (!Directory.EnumerateFiles(ror2Path, RoR2Executable).Any())
                ror2Path = EditorUtility.OpenFolderPanel("Open Risk of Rain 2 Root Folder", Directory.GetCurrentDirectory(), RoR2Executable);

            settings.RoR2Path = ror2Path;
            settings.SetDirty();

            foreach (var asm in RequiredAssemblies)
            {
                var destinationFile = Path.Combine(projectDirectory, "Assets", "Assemblies", $"{asm}.dll");
                var assemblyPath = Path.Combine(ror2Path, "Risk of Rain 2_Data", "Managed", $"{asm}.dll");
                var destinationMetaData = Path.Combine(projectDirectory, "Assets", "Assemblies", $"{asm}.meta");

                if (File.Exists(destinationFile)) File.Delete(destinationFile);
                File.Copy(assemblyPath, destinationFile);

                File.WriteAllText(destinationMetaData, MetaData);
            }

            _ = Editor.BepInExPackLoader.DownloadBepinex();

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
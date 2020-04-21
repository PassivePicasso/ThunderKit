//using System;
//using System.IO;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;

//namespace RainOfStages.AutoConfig
//{
//    [InitializeOnLoad]
//    public class ConfigureProject
//    {
//        private const string RoR2Executable = "Risk of Rain 2.exe";

//        static ConfigureProject() => EditorApplication.update += OnUpdate;

//        private static void OnUpdate()
//        {
//            const string AsmCSharp = "Assembly-CSharp.dll";
//            var settings = RainOfStagesSettings.GetOrCreateSettings();
//            var results = AssetDatabase.FindAssets("Assembly-CSharp");

//            if (results.Any() && !string.IsNullOrEmpty(settings.RoR2Path) && File.Exists(Path.Combine(settings.RoR2Path, "Risk of Rain 2.exe")))
//                return;

//            Debug.Log("Acquiring path for Rain of Stages");

//            string projectDirectory = Directory.GetCurrentDirectory();
//            string ror2Path = projectDirectory;
//            while (!Directory.EnumerateFiles(ror2Path, RoR2Executable).Any())
//                ror2Path = EditorUtility.OpenFolderPanel("Open Risk of Rain 2 Root Folder", Directory.GetCurrentDirectory(), RoR2Executable);

//            settings.RoR2Path = ror2Path;
//            settings.SetDirty();

//            var assemblyPath = Path.Combine(ror2Path, "Risk of Rain 2_Data", "Managed", AsmCSharp);
//            var destination = Path.Combine(projectDirectory, "Assets", "Assemblies", AsmCSharp);
//            var destinationMetaData = Path.Combine(projectDirectory, "Assets", "Assemblies", $"{AsmCSharp}.meta");


//            if (File.Exists(destination)) File.Delete(destination);
//            File.Copy(assemblyPath, destination);

//            File.WriteAllText(destinationMetaData, MetaData);
//            AssetDatabase.Refresh();
//        }

//        const string MetaData =
//@"fileFormatVersion: 2
//guid: fc64261ca6282254da01b0f016bcfcea
//PluginImporter:
//  externalObjects: {}
//  serializedVersion: 2
//  iconMap: {}
//  executionOrder: {}
//  defineConstraints: []
//  isPreloaded: 0
//  isOverridable: 0
//  isExplicitlyReferenced: 1
//  validateReferences: 0
//  platformData:
//  - first:
//      '': Any
//    second:
//      enabled: 0
//      settings:
//        Exclude Editor: 0
//        Exclude Linux: 0
//        Exclude Linux64: 0
//        Exclude LinuxUniversal: 0
//        Exclude OSXUniversal: 0
//        Exclude Win: 0
//        Exclude Win64: 0
//  - first:
//      Any: 
//    second:
//      enabled: 1
//      settings: {}
//  - first:
//      Editor: Editor
//    second:
//      enabled: 1
//      settings:
//        CPU: AnyCPU
//        DefaultValueInitialized: true
//        OS: AnyOS
//  - first:
//      Facebook: Win
//    second:
//      enabled: 0
//      settings:
//        CPU: AnyCPU
//  - first:
//      Facebook: Win64
//    second:
//      enabled: 0
//      settings:
//        CPU: AnyCPU
//  - first:
//      Standalone: Linux
//    second:
//      enabled: 1
//      settings:
//        CPU: x86
//  - first:
//      Standalone: Linux64
//    second:
//      enabled: 1
//      settings:
//        CPU: x86_64
//  - first:
//      Standalone: LinuxUniversal
//    second:
//      enabled: 1
//      settings: {}
//  - first:
//      Standalone: OSXUniversal
//    second:
//      enabled: 1
//      settings:
//        CPU: AnyCPU
//  - first:
//      Standalone: Win
//    second:
//      enabled: 1
//      settings:
//        CPU: AnyCPU
//  - first:
//      Standalone: Win64
//    second:
//      enabled: 1
//      settings:
//        CPU: AnyCPU
//  - first:
//      Windows Store Apps: WindowsStoreApps
//    second:
//      enabled: 0
//      settings:
//        CPU: AnyCPU
//  userData: 
//  assetBundleName: 
//  assetBundleVariant: 
//";
//    }
//}
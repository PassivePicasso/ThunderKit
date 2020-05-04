#if UNITY_EDITOR
using RainOfStages.Thunderstore;
using UnityEditorInternal;
using UnityEngine;
using RainOfStages.Utilities;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System.Linq;
using RainOfStages.AutoConfig;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace RainOfStages.Deploy
{
    public enum DeploymentType { Package, Test }
    public class Deployment : ScriptableObject
    {
        public Manifest Manifest;
        public BuildAssetBundleOptions BuildOptions;
        public DeploymentType DeploymentType;

        public AssemblyDefinitionAsset[] Plugins;
        public AssemblyDefinitionAsset[] Patchers;
        public AssemblyDefinitionAsset[] Monomod;


        public Texture2D Icon;

        private string[] Bundles;


        [MenuItem("Assets/Rain of Stages/" + nameof(Deployment))]
        public static void Create() => ScriptableHelper.CreateAsset<Deployment>();

        [MenuItem("Assets/Run Deployment", isValidateFunction: true)]
        public static bool CanDeploy() => Selection.activeObject is Deployment;

        [MenuItem("Assets/Run Deployment")]
        public static void Deploy()
        {
            Deployment deployment = Selection.activeObject as Deployment;

            var currentDir = Directory.GetCurrentDirectory();
            var scriptAssemblies = Path.Combine(currentDir, "Library", "ScriptAssemblies");

            var deployments = "Deployments";
            var outputPath = $"{deployments}/{deployment.name}";


            if (!Directory.Exists(deployments)) Directory.CreateDirectory(deployments);

            if (Directory.Exists(outputPath)) Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);


            switch (deployment.DeploymentType)
            {
                case DeploymentType.Package:
                    {
                        var pluginPath = Path.Combine(outputPath, "plugins", deployment.Manifest.name);
                        var patcherPath = Path.Combine(outputPath, "patchers", deployment.Manifest.name);
                        var monomodPath = Path.Combine(outputPath, "monomod", deployment.Manifest.name);

                        if (deployment.Plugins.Any() && !Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);
                        if (deployment.Patchers.Any() && !Directory.Exists(patcherPath)) Directory.CreateDirectory(patcherPath);
                        if (deployment.Monomod.Any() && !Directory.Exists(monomodPath)) Directory.CreateDirectory(monomodPath);

                        BuildPipeline.BuildAssetBundles(pluginPath, deployment.BuildOptions, BuildTarget.StandaloneWindows);

                        CopyReferences(deployment.Plugins, pluginPath);
                        CopyReferences(deployment.Patchers, patcherPath);
                        CopyReferences(deployment.Monomod, monomodPath);

                        var manifestJson = JsonUtility.ToJson(deployment.Manifest);
                        File.WriteAllText(Path.Combine(outputPath, "manifest.json"), manifestJson);

                        if (deployment.Icon)
                            File.WriteAllBytes(Path.Combine(outputPath, "icon.png"), deployment.Icon.EncodeToPNG());

                        File.WriteAllText(Path.Combine(outputPath, "README.md"), $"# {deployment.Manifest.name}");

                        ZipFile.CreateFromDirectory(outputPath, Path.Combine(deployments, $"{deployment.Manifest.name}.zip"));
                    }
                    break;
                case DeploymentType.Test:
                    {
                        var settings = RainOfStagesSettings.GetOrCreateSettings();
                        var bepinexPath = Path.Combine(settings.RoR2Path, "BepInEx");

                        var pluginPath = Path.Combine(bepinexPath, "plugins", deployment.Manifest.name);
                        var patcherPath = Path.Combine(bepinexPath, "patchers", deployment.Manifest.name);
                        var monomodPath = Path.Combine(bepinexPath, "monomod", deployment.Manifest.name);

                        if (deployment.Plugins.Any() && !Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);
                        if (deployment.Patchers.Any() && !Directory.Exists(patcherPath)) Directory.CreateDirectory(patcherPath);
                        if (deployment.Monomod.Any() && !Directory.Exists(monomodPath)) Directory.CreateDirectory(monomodPath);

                        BuildPipeline.BuildAssetBundles(pluginPath, deployment.BuildOptions, BuildTarget.StandaloneWindows);

                        CopyReferences(deployment.Plugins, pluginPath);
                        CopyReferences(deployment.Patchers, patcherPath);
                        CopyReferences(deployment.Monomod, monomodPath);


                        Debug.Log("Launching Risk of Rain 2");
                        var execPath = Path.Combine(settings.RoR2Path, "Risk of Rain 2.exe");
                        var psi = new ProcessStartInfo(execPath)
                        {
                            WorkingDirectory = settings.RoR2Path,
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        };
                        var process = new Process
                        {
                            StartInfo = psi
                        };
                        process.OutputDataReceived += Process_OutputDataReceived;
                        process.Start();
                        process.BeginOutputReadLine();
                    }
                    break;
            }

            AssetDatabase.Refresh();

            void CopyReferences(AssemblyDefinitionAsset[] assemblyDefs, string assemblyOutputPath)
            {
                foreach (var plugin in assemblyDefs)
                {
                    var assemblyDef = JsonUtility.FromJson<AssemblyDef>(plugin.text);
                    var fileName = $"{assemblyDef.name}.dll";
                    var fileOutput = Path.Combine(assemblyOutputPath, fileName);
                    var metaFileOutput = Path.Combine(assemblyOutputPath, $"{fileName}.meta");

                    if (File.Exists(fileOutput)) File.Delete(fileOutput);
                    if (File.Exists(metaFileOutput)) File.Delete(metaFileOutput);

                    File.Copy(Path.Combine(scriptAssemblies, fileName), fileOutput);
                    File.WriteAllText(Path.Combine(assemblyOutputPath, metaFileOutput), MetaData);
                }
            }
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.Log(e.Data);
        }

        internal const string MetaData =
@"fileFormatVersion: 2
guid: 7016fe7d899b67946bc3ebef577a743b
PluginImporter:
  externalObjects: {}
  serializedVersion: 2
  iconMap: {}
  executionOrder: {}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 0
  isExplicitlyReferenced: 1
  validateReferences: 0
  platformData:
  - first:
      '': Any
    second:
      enabled: 0
      settings:
        Exclude Editor: 1
        Exclude Linux: 1
        Exclude Linux64: 1
        Exclude LinuxUniversal: 1
        Exclude OSXUniversal: 1
        Exclude Win: 1
        Exclude Win64: 1
  - first:
      Any: 
    second:
      enabled: 0
      settings: {}
  - first:
      Editor: Editor
    second:
      enabled: 0
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
      enabled: 0
      settings:
        CPU: x86
  - first:
      Standalone: Linux64
    second:
      enabled: 0
      settings:
        CPU: x86_64
  - first:
      Standalone: LinuxUniversal
    second:
      enabled: 0
      settings:
        CPU: None
  - first:
      Standalone: OSXUniversal
    second:
      enabled: 0
      settings:
        CPU: AnyCPU
  - first:
      Standalone: Win
    second:
      enabled: 0
      settings:
        CPU: AnyCPU
  - first:
      Standalone: Win64
    second:
      enabled: 0
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
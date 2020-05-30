#if UNITY_EDITOR
using RainOfStages.AutoConfig;
using RainOfStages.Thunderstore;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RainOfStages.Deploy
{
    [Flags]
    public enum DeploymentOptions
    {
        None = 0,
        BuildBundles = 1,
        InstallBepInEx = 2,
        Package = 4,
        Clean = 8,
        InstallDependencies = 16,
        DeployMdbFiles = 32,
        ShowConsole = 64,
        Run = 8192,
    }

    public class Deployment : ScriptableObject
    {
        public Manifest Manifest;

        [EnumFlag]
        public BuildAssetBundleOptions AssetBundleBuildOptions;

        [EnumFlag]
        public DeploymentOptions DeploymentOptions;

        public AssemblyDefinitionAsset[] Plugins;
        public AssemblyDefinitionAsset[] Patchers;
        public AssemblyDefinitionAsset[] Monomod;
        public TextAsset Readme;

        public Texture2D Icon;

        private static string configTemplate = null;

        [MenuItem("Assets/Run Deployment", isValidateFunction: true)]
        public static bool CanDeploy() => Selection.activeObject is Deployment;

        [MenuItem("Assets/Run Deployment")]
        public async static void Deploy()
        {
            Deployment deployment = Selection.activeObject as Deployment;

            var settings = ThunderKitSettings.GetOrCreateSettings();
            var currentDir = Directory.GetCurrentDirectory();
            var dependencies = Path.Combine(currentDir, "Assets", "Dependencies");
            var scriptAssemblies = Path.Combine(currentDir, "Library", "ScriptAssemblies");
            var thunderPacks = Path.Combine(currentDir, "ThunderPacks", deployment.name);
            var tmpDir = Path.Combine(thunderPacks, "tmp");
            var bepinexPackDir = Path.Combine(thunderPacks, "BepInExPack");
            var bepinexDir = Path.Combine(thunderPacks, "BepInExPack", "BepInEx");
            var bepinexCoreDir = Path.Combine(bepinexDir, "core");
            var deployments = "Deployments";
            var outputPath = $"{deployments}/{deployment.name}";

            if (deployment.DeploymentOptions.HasFlag(DeploymentOptions.InstallBepInEx)
            || (!Directory.Exists(bepinexPackDir) 
                && (deployment.DeploymentOptions.HasFlag(DeploymentOptions.InstallDependencies) 
                    || deployment.DeploymentOptions.HasFlag(DeploymentOptions.DeployMdbFiles)
                    || deployment.DeploymentOptions.HasFlag(DeploymentOptions.Run))
               ))
            {
                if (Directory.Exists(bepinexPackDir)) Directory.Delete(bepinexPackDir, true);

                var bepinexPacks = await ThunderLoad.LookupPackage("BepInExPack");
                var bepinex = bepinexPacks.FirstOrDefault();

                string filePath = Path.Combine(currentDir, $"{bepinex.full_name}.zip");
                await ThunderLoad.DownloadPackageAsync(bepinex, filePath);

                using (var fileStream = File.OpenRead(filePath))
                using (var archive = new ZipArchive(fileStream))
                    archive.ExtractToDirectory(tmpDir);

                Directory.Move(Path.Combine(tmpDir, "BepInExPack"), Path.Combine(thunderPacks, "BepInExPack"));
                Directory.Delete(tmpDir, true);
                Debug.Log("Rebuilt Bepinex dir");

                string configPath = Path.Combine(bepinexDir, "Config", "BepInEx.cfg");
                File.Delete(configPath);
                string contents = GetBepInExConfig(deployment.DeploymentOptions.HasFlag(DeploymentOptions.ShowConsole));
                File.WriteAllText(configPath, contents);
            }

            if (File.Exists(Path.Combine(bepinexPackDir, "doorstop_config.ini")))
                File.Delete(Path.Combine(bepinexPackDir, "doorstop_config.ini"));

            if (File.Exists(Path.Combine(bepinexPackDir, "winhttp.dll"))
            && !File.Exists(Path.Combine(settings.GamePath, "winhttp.dll")))
                File.Copy(Path.Combine(bepinexPackDir, "winhttp.dll"),
                          Path.Combine(settings.GamePath, "winhttp.dll"));

            if (deployment.DeploymentOptions.HasFlag(DeploymentOptions.Clean))
            {
                CleanManifestFiles(bepinexDir);
                CleanManifestFiles(outputPath);
            }

            if (deployment.DeploymentOptions.HasFlag(DeploymentOptions.InstallDependencies))
            {
                if (Directory.Exists(dependencies))
                {
                    var dependencyDirs = Directory.EnumerateDirectories(dependencies, "*", searchOption: SearchOption.TopDirectoryOnly).ToArray();

                    foreach (var modDir in dependencyDirs)
                    {
                        string patcher = Path.Combine(modDir, "patchers");
                        string plugins = Path.Combine(modDir, "plugins");
                        string monomod = Path.Combine(modDir, "monomod");

                        if (!Directory.Exists(patcher) && !Directory.Exists(plugins) && !Directory.Exists(monomod))
                        {
                            CopyFilesRecursively(modDir, Path.Combine(bepinexDir, "plugins"));
                        }
                        else
                        {
                            if (Directory.Exists(patcher)) CopyFilesRecursively(patcher, Path.Combine(bepinexDir, "patchers"));
                            if (Directory.Exists(plugins)) CopyFilesRecursively(plugins, Path.Combine(bepinexDir, "plugins"));
                            if (Directory.Exists(monomod)) CopyFilesRecursively(monomod, Path.Combine(bepinexDir, "monomod"));
                        }
                    }
                }
            }

            if (!Directory.Exists(deployments)) Directory.CreateDirectory(deployments);
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            if (deployment.DeploymentOptions.HasFlag(DeploymentOptions.Package))
            {
                CopyAllReferences(outputPath);

                var manifestJson = JsonUtility.ToJson(deployment.Manifest);

                manifestJson = manifestJson.Substring(1);
                manifestJson = $"{{\"name\":\"{deployment.Manifest.name}\",{manifestJson}";
                File.WriteAllText(Path.Combine(outputPath, "manifest.json"), manifestJson);

                var readmePath = AssetDatabase.GetAssetPath(deployment.Readme);
                File.Copy(readmePath, Path.Combine(outputPath, Path.GetFileName(readmePath)), true);

                if (deployment.Icon)
                    File.WriteAllBytes(Path.Combine(outputPath, "icon.png"), deployment.Icon.EncodeToPNG());

                File.WriteAllText(Path.Combine(outputPath, "README.md"), $"# {deployment.Manifest.name}");

                string outputFile = Path.Combine(deployments, $"{deployment.Manifest.name}.zip");
                if (File.Exists(outputFile)) File.Delete(outputFile);

                ZipFile.CreateFromDirectory(outputPath, outputFile);
            }

            var ror2Executable = Path.Combine(settings.GamePath, "Risk of Rain 2.exe");
            if (deployment.DeploymentOptions.HasFlag(DeploymentOptions.Run))
            {
                CopyAllReferences(bepinexDir);
                Debug.Log("Launching Risk of Rain 2");

                var rorPsi = new ProcessStartInfo(ror2Executable)
                {
                    WorkingDirectory = bepinexDir,
                    Arguments = $"--doorstop-enable true --doorstop-target \"{Path.Combine(bepinexCoreDir, "BepInEx.Preloader.dll")}\"",
                    //Standard output redirection doesn't currently work with bepinex, appears to be considered a bepinex bug
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                //string connectionAddress = "127.0.0.1";
                //string connectionPort = "55555";
                //rorPsi.Environment["DNSPY_UNITY_DBG"] = $"--debugger-agent=transport=dt_socket,server=y,address={connectionAddress}:{connectionPort},defer=y,no-hide-debugger";
                //rorPsi.Environment["DNSPY_UNITY_DBG2"] = $"--debugger-agent=transport=dt_socket,server=y,address={connectionAddress}:{connectionPort},suspend=n,no-hide-debugger";
                //var envVariables = Environment.GetEnvironmentVariables();
                //foreach (var variable in envVariables.Keys.OfType<string>())
                //    rorPsi.Environment[variable] = envVariables[variable] as string;

                var rorProcess = new Process { StartInfo = rorPsi };

                //process.OutputDataReceived += Process_OutputDataReceived;
                //process.BeginOutputReadLine();
                rorProcess.Start();
            }

            AssetDatabase.Refresh();

            void CleanManifestFiles(string rootPath)
            {
                var pluginPath = Path.Combine(rootPath, "plugins", deployment.Manifest.name);
                var patcherPath = Path.Combine(rootPath, "patchers", deployment.Manifest.name);
                var monomodPath = Path.Combine(rootPath, "monomod", deployment.Manifest.name);

                if (Directory.Exists(pluginPath)) Directory.Delete(pluginPath, true);
                if (Directory.Exists(patcherPath)) Directory.Delete(patcherPath, true);
                if (Directory.Exists(monomodPath)) Directory.Delete(monomodPath, true);
            }

            void CopyReferences(AssemblyDefinitionAsset[] assemblyDefs, string assemblyOutputPath)
            {
                foreach (var plugin in assemblyDefs)
                {
                    var assemblyDef = JsonUtility.FromJson<AssemblyDef>(plugin.text);
                    var fileOutputBase = Path.Combine(assemblyOutputPath, assemblyDef.name);
                    var fileName = Path.GetFileName(fileOutputBase);
                    if (File.Exists($"{fileOutputBase}.dll")) File.Delete($"{fileOutputBase}.dll");

                    File.Copy(Path.Combine(scriptAssemblies, $"{assemblyDef.name}.dll"), $"{fileOutputBase}.dll");

                    if (deployment.DeploymentOptions.HasFlag(DeploymentOptions.DeployMdbFiles))
                    {

                        string monodatabase = $"{Path.Combine(scriptAssemblies, fileName)}.dll.mdb";
                        if (File.Exists(monodatabase))
                        {
                            if (File.Exists($"{fileOutputBase}.dll.mdb")) File.Delete($"{fileOutputBase}.dll.mdb");

                            File.Copy(monodatabase, $"{fileOutputBase}.dll.mdb");
                        }

                        string programdatabase = Path.Combine(scriptAssemblies, $"{fileName}.pdb");
                        if (File.Exists(programdatabase))
                        {
                            if (File.Exists($"{fileOutputBase}.pdb")) File.Delete($"{fileOutputBase}.pdb");

                            File.Copy(programdatabase, $"{fileOutputBase}.pdb");
                        }
                    }
                }
            }

            void CopyAllReferences(string rootPath)
            {
                var pluginPath = Path.Combine(rootPath, "plugins", deployment.Manifest.name);
                var patcherPath = Path.Combine(rootPath, "patchers", deployment.Manifest.name);
                var monomodPath = Path.Combine(rootPath, "monomod", deployment.Manifest.name);

                if (deployment.Plugins.Any() && !Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);
                if (deployment.Patchers.Any() && !Directory.Exists(patcherPath)) Directory.CreateDirectory(patcherPath);
                if (deployment.Monomod.Any() && !Directory.Exists(monomodPath)) Directory.CreateDirectory(monomodPath);

                if (deployment.DeploymentOptions.HasFlag(DeploymentOptions.BuildBundles))
                    BuildPipeline.BuildAssetBundles(pluginPath, deployment.AssetBundleBuildOptions, BuildTarget.StandaloneWindows);

                CopyReferences(deployment.Plugins, pluginPath);
                CopyReferences(deployment.Patchers, patcherPath);
                CopyReferences(deployment.Monomod, monomodPath);
            }

#warning This is to setup the Bepinex Config until bepinex is updated
            string GetBepInExConfig(bool consoleEnabled)
            {
                string configTemplatePath = Path.Combine(currentDir, "ThunderKit", "Deploy", "Editor", "configtemplate.txt");

                if (configTemplate == null)
                    configTemplate = File.ReadAllText(configTemplatePath);

                return string.Format(configTemplate, consoleEnabled.ToString().ToLower());
            }
        }


        public static void CopyFilesRecursively(string source, string target)
        {
            foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).Where(f => !f.EndsWith("meta")))
            {
                var parentDirectory = Path.GetFileName(Path.GetDirectoryName(file));
                var targetParent = Path.GetFileName(target);
                var subdirectory = parentDirectory.Equals(targetParent) ? target : Path.Combine(target, parentDirectory);
                Directory.CreateDirectory(subdirectory);
                File.Copy(file, Path.Combine(subdirectory, Path.GetFileName(file)), true);
            }
        }
        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.Log(e.Data);
        }

    }
}
#endif
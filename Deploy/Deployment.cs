//#if UNITY_EDITOR
//using PassivePicasso.ThunderKit.Editor;
//using PassivePicasso.ThunderKit.Thunderstore.Editor;
//using PassivePicasso.ThunderKit.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.Threading.Tasks;
//using UnityEditor;
//using UnityEditor.Callbacks;
//using UnityEditorInternal;
//using UnityEngine;
//using Debug = UnityEngine.Debug;

//namespace PassivePicasso.ThunderKit.Deploy.Editor
//{
//    [Flags]
//    public enum DeploymentOptions
//    {
//        None = 0,
//        BuildBundles = 1,
//        BuildRedistributables = 256,
//        InstallBepInEx = 2,
//        Package = 4,
//        Clean = 8,
//        InstallDependencies = 16,
//        DeployMdbFiles = 32,
//        DeployAssemblies = 128,
//        ShowConsole = 64,
//        Run = 8192,
//    }

//    [Flags]
//    public enum LogLevel
//    {
//        //Disables all log messages
//        None = 0,
//        //Errors which cannot be recovered from; the game cannot continue to run
//        Fatal = 1,
//        //Errors are recoverable from; the game can be run, albeit with further errors
//        Error = 2,
//        //Messages that signify an anomaly that is not an error
//        Warning = 4,
//        //Important messages that should be displayed
//        Message = 8,
//        //Messages of low importance
//        Info = 16,
//        //Messages intended for developers
//        Debug = 32,

//        //All = Fatal | Error | Warning | Message | Info | Debug
//    }

//    public class Deployment : ScriptableObject
//    {
//        public Manifest Manifest;

//        [EnumFlag]
//        public BuildAssetBundleOptions AssetBundleBuildOptions;

//        [EnumFlag]
//        public DeploymentOptions DeploymentOptions;

//        [EnumFlag]
//        public LogLevel LogLevel;

//        [SerializeField]
//        public string[] extraCommandLineArgs;

//        public TextAsset Readme => Manifest.readme;

//        public Texture2D Icon => Manifest.icon;

//        public bool ExecuteOnDoubleClick;

//        [MenuItem("Assets/Run Deployment", isValidateFunction: true)]
//        public static bool CanDeploy() => Selection.activeObject is Deployment;

//        [MenuItem("Assets/Run Deployment")]
//        public static void ContextDeploy()
//        {
//            Deployment deployment = Selection.activeObject as Deployment;
//            Deploy(deployment);
//        }

//        [OnOpenAsset]
//        public static bool DoubleClickDeploy(int instanceID, int line)
//        {
//            if (!(EditorUtility.InstanceIDToObject(instanceID) is Deployment instance)) return false;

//            if (instance.ExecuteOnDoubleClick) Deploy(instance);

//            return instance.ExecuteOnDoubleClick;
//        }

//        public async static void Deploy(Deployment deployment)
//        {
//            if (deployment == null) return;

//            var deployments/*    */= "Deployments";
//            var settings/*       */= ThunderKitSettings.GetOrCreateSettings();
//            var dependencies/*   */= Path.Combine("Assets", "Dependencies");
//            var thunderPacks/*   */= Path.Combine("ThunderPacks", deployment.Manifest.name);
//            var tmpDir/*         */= Path.Combine(thunderPacks, "tmp");
//            var bepinexPackDir/* */= Path.Combine(thunderPacks, "BepInExPack");
//            var bepinexDir/*     */= Path.Combine(thunderPacks, "BepInExPack", "BepInEx");
//            var bepinexCoreDir/* */= Path.Combine(bepinexDir, "core");
//            var outputPath/*     */= Path.Combine(deployments, deployment.Manifest.name);

//            bool clean/*               */= deployment.DeploymentOptions.HasFlag(DeploymentOptions.Clean);
//            bool installBepinex/*      */= deployment.DeploymentOptions.HasFlag(DeploymentOptions.InstallBepInEx);
//            bool installDependencies/* */= deployment.DeploymentOptions.HasFlag(DeploymentOptions.InstallDependencies);
//            bool deployMdbs/*          */= deployment.DeploymentOptions.HasFlag(DeploymentOptions.DeployMdbFiles);
//            bool run/*                 */= deployment.DeploymentOptions.HasFlag(DeploymentOptions.Run);
//            bool buildBundles/*        */= deployment.DeploymentOptions.HasFlag(DeploymentOptions.BuildBundles);
//            var buildRedistributables/**/= deployment.DeploymentOptions.HasFlag(DeploymentOptions.BuildRedistributables);
//            bool package/*             */= deployment.DeploymentOptions.HasFlag(DeploymentOptions.Package);

//            if (clean)
//            {
//                CleanManifestFiles(outputPath, deployment.Manifest);
//                CleanManifestFiles(bepinexDir, deployment.Manifest);
//            }

//            if (!Directory.Exists(deployments)) Directory.CreateDirectory(deployments);
//            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

//            await SetupBepinEx(deployment, thunderPacks, tmpDir, bepinexPackDir, bepinexDir, installBepinex, installDependencies, deployMdbs, run);

//            if (File.Exists(Path.Combine(bepinexPackDir, "winhttp.dll")))
//                File.Copy(Path.Combine(bepinexPackDir, "winhttp.dll"),
//                          Path.Combine(settings.GamePath, "winhttp.dll"), true);

//            if (buildRedistributables)
//            {
//                foreach (var redistributable in deployment.Manifest.redistributables)
//                {
//                    Redistributable.Export(redistributable, outputPath);
//                }
//            }

//            InstallDependencies(deployment, dependencies, bepinexDir, installDependencies);

//            settings = BuildBundles(deployment, settings, bepinexDir, outputPath, buildBundles, package);

//            Package(deployment, deployments, outputPath, package);

//            Run(deployment, settings, bepinexDir, bepinexCoreDir, run);

//            AssetDatabase.Refresh();
//        }

//        private static async Task SetupBepinEx(Deployment deployment, string thunderPacks, string tmpDir, string bepinexPackDir, string bepinexDir, bool installBepinex, bool installDependencies, bool deployMdbs, bool run)
//        {
//            if (installBepinex || (!Directory.Exists(bepinexPackDir) && (installDependencies || deployMdbs || run)))
//            {
//                if (installBepinex && Directory.Exists(bepinexPackDir))
//                    Directory.Delete(bepinexPackDir, true);

//                var bepinexPacks = ThunderLoad.LookupPackage("BepInExPack");
//                var bepinex = bepinexPacks.FirstOrDefault();

//                var filePath = $"{bepinex.full_name}.zip";
//                await ThunderLoad.DownloadPackageAsync(bepinex, filePath);

//                using (var fileStream = File.OpenRead(filePath))
//                using (var archive = new ZipArchive(fileStream))
//                    archive.ExtractToDirectory(tmpDir);

//                Directory.Move(Path.Combine(tmpDir, "BepInExPack"), Path.Combine(thunderPacks, "BepInExPack"));
//                Directory.Delete(tmpDir, true);
//                Debug.Log("Rebuilt Bepinex dir");
//            }

//            string configPath = Path.Combine(bepinexDir, "Config", "BepInEx.cfg");
//            if (Directory.Exists(Path.Combine(bepinexDir, "Config")))
//            {
//                File.Delete(configPath);
//                var logLevels = deployment.LogLevel.GetFlags().Select(f => $"{f}").Aggregate((a, b) => $"{a}, {b}");
//                string contents = ConfigTemplate.CreatBepInExConfig(deployment.DeploymentOptions.HasFlag(DeploymentOptions.ShowConsole), logLevels);
//                File.WriteAllText(configPath, contents);
//            }
//        }

//        private static void Run(Deployment deployment, ThunderKitSettings settings, string bepinexDir, string bepinexCoreDir, bool run)
//        {
//            var ror2Executable = Path.Combine(settings.GamePath, settings.GameExecutable);
//            if (run)
//            {
//                if (File.Exists(Path.Combine(settings.GamePath, "doorstop_config.ini")))
//                    File.Move(Path.Combine(settings.GamePath, "doorstop_config.ini"), Path.Combine(settings.GamePath, "doorstop_config.bak.ini"));

//                CopyAllReferences(bepinexDir, deployment);
//                Debug.Log($"Launching {Path.GetFileNameWithoutExtension(settings.GameExecutable)}");
//                var arguments = new List<string>
//                {
//                    "--doorstop-enable true",
//                    $"--doorstop-target \"{Path.Combine(Directory.GetCurrentDirectory(), bepinexCoreDir, "BepInEx.Preloader.dll")}\""
//                };
//                if (deployment.extraCommandLineArgs?.Any() ?? false)
//                    arguments.AddRange(deployment.extraCommandLineArgs);

//                var args = arguments.Aggregate((a, b) => $"{a} {b}");

//                var rorPsi = new ProcessStartInfo(ror2Executable)
//                {
//                    WorkingDirectory = bepinexDir,
//                    Arguments = args,

//                    //Standard output redirection doesn't currently work with bepinex, appears to be considered a bepinex bug
//                    //RedirectStandardOutput = true,
//                    UseShellExecute = true
//                };

//                var rorProcess = new Process { StartInfo = rorPsi, EnableRaisingEvents = true };
//                EventHandler RorProcess_Exited = null;
//                RorProcess_Exited = new EventHandler((object sender, EventArgs e) =>
//                {
//                    rorProcess.Exited -= RorProcess_Exited;
//                    if (File.Exists(Path.Combine(settings.GamePath, "doorstop_config.bak.ini")))
//                        File.Move(Path.Combine(settings.GamePath, "doorstop_config.bak.ini"), Path.Combine(settings.GamePath, "doorstop_config.ini"));
//                });
//                rorProcess.Exited += RorProcess_Exited;

//                rorProcess.Start();
//            }
//        }

//        private static void Package(Deployment deployment, string deployments, string outputPath, bool package)
//        {
//            if (package)
//            {
//                CopyAllReferences(outputPath, deployment);

//                if (deployment.Readme)
//                {
//                    var readmePath = AssetDatabase.GetAssetPath(deployment.Readme);
//                    File.Copy(readmePath, Path.Combine(outputPath, "README.md"), true);
//                }
//                else File.WriteAllText(Path.Combine(outputPath, "README.md"), $"# {deployment.Manifest.name}");


//                if (deployment.Icon)
//                    File.WriteAllBytes(Path.Combine(outputPath, "icon.png"), deployment.Icon.EncodeToPNG());

//                string outputFile = Path.Combine(deployments, $"{deployment.Manifest.name}.zip");
//                if (File.Exists(outputFile)) File.Delete(outputFile);

//                ZipFile.CreateFromDirectory(outputPath, outputFile);
//            }
//        }

//        private static void InstallDependencies(Deployment deployment, string dependencies, string bepinexDir, bool installDependencies)
//        {
//            if (installDependencies)
//            {
//                if (Directory.Exists(dependencies))
//                {
//                    var dependencyDirs = Directory.EnumerateDirectories(dependencies, "*", searchOption: SearchOption.TopDirectoryOnly).ToArray();

//                    foreach (var modDir in dependencyDirs)
//                    {
//                        if (!deployment.Manifest.dependencies.Contains(Path.GetFileName(modDir))) continue;

//                        string patcher = Path.Combine(modDir, "patchers");
//                        string plugins = Path.Combine(modDir, "plugins");
//                        string monomod = Path.Combine(modDir, "monomod");

//                        if (!Directory.Exists(patcher) && !Directory.Exists(plugins) && !Directory.Exists(monomod))
//                        {
//                            CopyFilesRecursively(modDir, Path.Combine(bepinexDir, "plugins"));
//                        }
//                        else
//                        {
//                            if (Directory.Exists(patcher)) CopyFilesRecursively(patcher, Path.Combine(bepinexDir, "patchers"));
//                            if (Directory.Exists(plugins)) CopyFilesRecursively(plugins, Path.Combine(bepinexDir, "plugins"));
//                            if (Directory.Exists(monomod)) CopyFilesRecursively(monomod, Path.Combine(bepinexDir, "monomod"));
//                        }
//                    }
//                }
//            }
//        }

//        private static ThunderKitSettings BuildBundles(Deployment deployment, ThunderKitSettings settings, string bepinexDir, string outputPath, bool buildBundles, bool package)
//        {
//            if (buildBundles)
//            {
//                var targetPath = package ? Path.Combine(outputPath, "plugins", deployment.Manifest.name)
//                                         : Path.Combine(bepinexDir, "plugins", deployment.Manifest.name);
//                if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
//                BuildPipeline.BuildAssetBundles(targetPath, deployment.AssetBundleBuildOptions, BuildTarget.StandaloneWindows);

//                //Domain reload causes loss of settings object
//                if (!settings)
//                    settings/*       */= ThunderKitSettings.GetOrCreateSettings();
//            }

//            return settings;
//        }

//        static void CleanManifestFiles(string rootPath, Manifest manifest)
//        {
//            var pluginPath = Path.Combine(rootPath, "plugins", manifest.name);
//            var patcherPath = Path.Combine(rootPath, "patchers", manifest.name);
//            var monomodPath = Path.Combine(rootPath, "monomod", manifest.name);

//            if (Directory.Exists(pluginPath)) Directory.Delete(pluginPath, true);
//            if (Directory.Exists(patcherPath)) Directory.Delete(patcherPath, true);
//            if (Directory.Exists(monomodPath)) Directory.Delete(monomodPath, true);
//        }

//        static void CopyAllReferences(string outputRoot, Deployment deployment)
//        {
//            var manifest = deployment.Manifest;
//            var pluginPath = Path.Combine(outputRoot, "plugins", manifest.name);
//            var patcherPath = Path.Combine(outputRoot, "patchers", manifest.name);
//            var monomodPath = Path.Combine(outputRoot, "monomod", manifest.name);

//            if (manifest.plugins.Any() && !Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);
//            if (manifest.patchers.Any() && !Directory.Exists(patcherPath)) Directory.CreateDirectory(patcherPath);
//            if (manifest.monomod.Any() && !Directory.Exists(monomodPath)) Directory.CreateDirectory(monomodPath);


//            CopyReferences(manifest.plugins, pluginPath, deployment);
//            CopyReferences(manifest.patchers, patcherPath, deployment);
//            CopyReferences(manifest.monomod, monomodPath, deployment);

//            var manifestJson = manifest.RenderJson();
//            if (Directory.Exists(pluginPath)) File.WriteAllText(Path.Combine(pluginPath, "manifest.json"), manifestJson);

//            if (deployment.DeploymentOptions.HasFlag(DeploymentOptions.Package))
//                File.WriteAllText(Path.Combine(outputRoot, "manifest.json"), manifestJson);

//            var settings = ThunderKitSettings.GetOrCreateSettings();
//            if (settings?.deployment_exclusions?.Any() ?? false)
//                foreach (var deployment_exclusion in settings.deployment_exclusions)
//                    foreach (var file in Directory.EnumerateFiles(pluginPath, deployment_exclusion, SearchOption.AllDirectories).ToArray())
//                        File.Delete(file);
//        }

//        static void CopyReferences(AssemblyDefinitionAsset[] assemblyDefs, string assemblyOutputPath, Deployment deployment)
//        {
//            var scriptAssemblies = Path.Combine("Library", "ScriptAssemblies");
//            bool deployAssemblies = deployment.DeploymentOptions.HasFlag(DeploymentOptions.DeployAssemblies);
//            bool deployMdbs = deployment.DeploymentOptions.HasFlag(DeploymentOptions.DeployMdbFiles);

//            if (!deployAssemblies && !deployMdbs) return;

//            foreach (var plugin in assemblyDefs)
//            {
//                var assemblyDef = JsonUtility.FromJson<AssemblyDef>(plugin.text);
//                var fileOutputBase = Path.Combine(assemblyOutputPath, assemblyDef.name);
//                var fileName = Path.GetFileName(fileOutputBase);

//                if (deployAssemblies)
//                {
//                    if (File.Exists($"{fileOutputBase}.dll")) File.Delete($"{fileOutputBase}.dll");
//                    File.Copy(Path.Combine(scriptAssemblies, $"{assemblyDef.name}.dll"), $"{fileOutputBase}.dll");
//                }

//                if (deployMdbs)
//                {
//                    string monodatabase = $"{Path.Combine(scriptAssemblies, fileName)}.dll.mdb";
//                    if (File.Exists(monodatabase))
//                    {
//                        if (File.Exists($"{fileOutputBase}.dll.mdb")) File.Delete($"{fileOutputBase}.dll.mdb");

//                        File.Copy(monodatabase, $"{fileOutputBase}.dll.mdb");
//                    }

//                    string programdatabase = Path.Combine(scriptAssemblies, $"{fileName}.pdb");
//                    if (File.Exists(programdatabase))
//                    {
//                        if (File.Exists($"{fileOutputBase}.pdb")) File.Delete($"{fileOutputBase}.pdb");

//                        File.Copy(programdatabase, $"{fileOutputBase}.pdb");
//                    }
//                }
//            }
//        }

//        public static void CopyFilesRecursively(string source, string target)
//        {
//            foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).Where(f => !f.EndsWith("meta")).ToArray())
//            {
//                var parentDirectory = Path.GetFileName(Path.GetDirectoryName(file));
//                var targetParent = Path.GetFileName(target);
//                var subdirectory = parentDirectory.Equals(targetParent) ? target : Path.Combine(target, parentDirectory);
//                Directory.CreateDirectory(subdirectory);
//                File.Copy(file, Path.Combine(subdirectory, Path.GetFileName(file)), true);
//            }
//        }
//        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            Debug.Log(e.Data);
//        }
//    }
//}
//#endif
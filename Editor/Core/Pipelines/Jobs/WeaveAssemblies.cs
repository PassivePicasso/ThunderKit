using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Manifests.Datums;
using UnityEngine;
using ThunderKit.Core.Paths;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace ThunderKit.WeaverKit
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor, RequiresManifestDatumType(typeof(AssemblyDefinitions))]
    public class WeaveAssemblies : PipelineJob
    {
        [Serializable]
        struct AsmDef
        {
            public string name;
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public bool autoReferenced;
            public string[] optionalUnityReferences;
            public string[] includePlatforms;
            public string[] excludePlatforms;
            public string[] precompiledReferences;
            public string[] defineConstraints;
        }

        [PathReferenceResolver, Tooltip("Location where the StageAssemblies job cache the assemblies before being staged")]
        public string assemblyArtifactPath = "<AssemblyStaging>";
        [Tooltip("If true, the weaved assemblies will  be cached on \"assemblyArtifactPath\\WeavedAssemblies\", otherwise theyre stored in \"assemblyArtifactPath\"")]
        public bool cacheAssembliesOnSubfolder = false;

        [HideInInspector]
        public string uNetAssemblyPath, unityEngineAssemblyPath = string.Empty;

        public static string WeaverDir = "<WeaverDir>";
        public static string WeaverExe = "<WeaverExe>";
        public static string WeaverTemp = "<WeaverTemp>";

        public override async Task Execute(Pipeline pipeline)
        {
            List<Task> tasks = new List<Task>();
            var resolvedArtifactPath = PathReference.ResolvePath(assemblyArtifactPath, pipeline, this);

            var definitionDatums = pipeline.Manifest.Data.OfType<AssemblyDefinitions>().ToArray();
            if (!definitionDatums.Any())
            {
                var scriptPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
                pipeline.Log(LogLevel.Warning, $"No AssemblyDefinitions found, skipping.");
                return;
            }

            for (int i = 0; i < definitionDatums.Length; i++)
            {
                var datum = definitionDatums[i];
                if (!datum)
                    continue;

                var hasUnassignedDefinition = datum.definitions.Any(def => !(bool)def);
                if (hasUnassignedDefinition)
                    pipeline.Log(LogLevel.Warning, $"AssemblyDefinitions with unassigned definition at index {i}");
            }

            var deserializedAsmDefs = definitionDatums.SelectMany(datum =>
                datum.definitions.Where(asmDefAsset => asmDefAsset)
                .Select(asmDefAsset => (asmDef: JsonUtility.FromJson<AsmDef>(asmDefAsset.text),
                datum: datum)));

            try
            {
                pipeline.Log(LogLevel.Information, $"Began setting up weaving enviroment.");
                string unityEngineArg = GetCoreDLL(pipeline);
                string uNetDLLArg = GetUNetAssemblyPath(pipeline);
                string outputArg = GetOutputPath(pipeline);
                string extraAssemblyArg = GetExtraAssemblies(pipeline);
                string[] assemblyToWeaveArgs = GetPathOfAssemblyToWeave(deserializedAsmDefs, resolvedArtifactPath, pipeline);
                pipeline.Log(LogLevel.Information, $"Weaving enviroment set up, attempting to weave a total of {assemblyToWeaveArgs.Length} assemblies.");

                foreach (string assemblyPath in assemblyToWeaveArgs)
                {
                    List<string> args = new List<string> { unityEngineArg, uNetDLLArg, outputArg, assemblyPath, extraAssemblyArg };
                    if (args.Any(path => string.IsNullOrEmpty(path)))
                    {
                        List<string> logBuilder = new List<string>
                        {
                            $"* Arg0 - UnityEngine.dll: {unityEngineArg}",
                            $"* Arg1 - HLAPI.Runtime.dll: {uNetDLLArg}",
                            $"* Arg2 - Output path: {outputArg}",
                            $"* Arg3 - Assembly to weave path: {assemblyPath}",
                            $"* Arg4 - Extra assemblies path: {extraAssemblyArg}"
                        };
                        pipeline.Log(LogLevel.Warning, $"Cannot weave assembly {Path.GetFileName(assemblyPath)}, as one of the arguments is null or empty.", logBuilder.ToArray());
                        continue;
                    }
                    tasks.Add(WeaveAssembly(args, pipeline));
                }
            }
            finally
            {
                string tempPath = PathReference.ResolvePath(WeaverTemp, pipeline, this);
                pipeline.Log(LogLevel.Information, $"Weaving process complete, disposing of temporary folder {tempPath}");
                if (Directory.Exists(Path.GetFullPath(tempPath)))
                {
                    Directory.Delete(Path.GetFullPath(tempPath), true);
                }
            }

            await Task.WhenAll(tasks);

            List<string> logger = new List<string>();
            int num = 0;
            foreach (var (asmDef, datum) in deserializedAsmDefs)
            {
                var assemblyName = $"{asmDef.name}.dll";
                var weavedAssemblyPath = cacheAssembliesOnSubfolder ? Path.Combine(resolvedArtifactPath, "WeavedAssemblies") : resolvedArtifactPath;
                var origPath = Path.Combine(weavedAssemblyPath, assemblyName);

                foreach (var stagingPath in datum.StagingPaths)
                {
                    string resolvedPath = PathReference.ResolvePath(stagingPath, pipeline, this);
                    string finalizedPath = Path.Combine(resolvedPath, assemblyName);

                    FileUtil.ReplaceFile(origPath, finalizedPath);
                    logger.Add($"* Moved weaved assembly {assemblyName} from ***{origPath}*** to ***{finalizedPath}***");
                    num++;
                }
            }
            pipeline.Log(LogLevel.Information, $"Moveed a total of {num} waved assemblies to the stagingPaths.", logger.ToArray());
        }
        private string GetCoreDLL(Pipeline pipeline)
        {
            //If serialized path is valid, use it. otherwise, try to find automatically.
            if (File.Exists(unityEngineAssemblyPath))
            {
                pipeline.Log(LogLevel.Information, $"Using {unityEngineAssemblyPath} for weaving process as arg0.");
                return unityEngineAssemblyPath;
            }

            //If the automatic attempt throws an exception, prompt user to select the dll manually
            try
            {
                string dataFolder = EditorApplication.applicationContentsPath;
                string managedFolder = Path.Combine(dataFolder, "Managed");

                if (!Directory.Exists(managedFolder))
                    throw new IOException($"Could not find path to Managed folder");

                string coreModulePath = Path.Combine(managedFolder, "UnityEngine.dll");

                if (!File.Exists(Path.GetFullPath(coreModulePath)))
                    throw new IOException($"Could not find path to the UnityEngine.dll inside the Managed folder.");

                unityEngineAssemblyPath = Path.GetFullPath(coreModulePath);

                pipeline.Log(LogLevel.Information, $"Using {unityEngineAssemblyPath} for weaving process as arg0.");
                return unityEngineAssemblyPath;
            }
            catch (Exception ex)
            {
                pipeline.Log(LogLevel.Error, $"Exception: {ex}. prompting user to manually select the Core dll.");
                string currentDir = EditorApplication.applicationContentsPath;
                var foundAssembly = false;

                while (!foundAssembly)
                {
                    var assemblyPath = string.Empty;
                    switch (Application.platform)
                    {
                        case RuntimePlatform.WindowsEditor:
                            assemblyPath = EditorUtility.OpenFilePanel("Select UnityEngine.dll", currentDir, ".dll");
                            break;
                        //case RuntimePlatform.LinuxEditor:
                        //    path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "*");
                        //    break;
                        //case RuntimePlatform.OSXEditor:
                        //    path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "app");
                        //    break;
                        default:
                            EditorUtility.DisplayDialog("Unsupported", "Your operating system is partially or completely unsupported. Contributions to improve this are welcome", "Ok");
                            return null;
                    }
                    if (string.IsNullOrEmpty(assemblyPath))
                    {
                        pipeline.Log(LogLevel.Warning, "Selected assembly path is null or empty.");
                        return null;
                    }
                    if (string.Compare(Path.GetFileName(assemblyPath), "UnityEngine.dll", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        pipeline.Log(LogLevel.Warning, "Selected assembly path's file's name is not \"UnityEngine.dll\"");
                        return null;
                    }

                    unityEngineAssemblyPath = assemblyPath;
                    foundAssembly = true;
                }
                pipeline.Log(LogLevel.Information, $"Using {unityEngineAssemblyPath} for weaving process as arg0.");
                return unityEngineAssemblyPath;
            }
        }

        private string GetUNetAssemblyPath(Pipeline pipeline)
        {
            if (File.Exists(uNetAssemblyPath))
            {
                pipeline.Log(LogLevel.Information, $"Using {uNetAssemblyPath} for weaving process as arg1.");
                return uNetAssemblyPath;
            }

            Assembly networkingAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetName().Name == "com.unity.multiplayer-hlapi.Runtime")
                .FirstOrDefault();

            if (networkingAssembly == null)
                throw new NullReferenceException($"Could not find the Multiplayer High Level API runtime dll. Weaving cannot be performed.");

            uNetAssemblyPath = networkingAssembly.Location;
            pipeline.Log(LogLevel.Information, $"Using {uNetAssemblyPath} for weaving process as arg1.");
            return uNetAssemblyPath;
        }

        private string GetOutputPath(Pipeline pipeline)
        {
            string resolvedArtifactPath = PathReference.ResolvePath(assemblyArtifactPath, pipeline, this);
            string fullArtifactPath = Path.GetFullPath(resolvedArtifactPath);

            string outputPath = cacheAssembliesOnSubfolder ? Path.Combine(fullArtifactPath, "WeavedAssemblies") : fullArtifactPath;
            pipeline.Log(LogLevel.Information, $"Using {outputPath} for weaving process as arg2");
            return outputPath;
        }

        //Absolutely cursed.
        private string GetExtraAssemblies(Pipeline pipeline)
        {
            string tempPath = PathReference.ResolvePath(WeaverTemp, pipeline, this);
            if (!Directory.Exists(tempPath))
            {
                pipeline.Log(LogLevel.Information, "Creating temp folder.");
                Directory.CreateDirectory(tempPath);
            }

            string fullTempPath = Path.GetFullPath(tempPath);
            if (!File.Exists(Path.Combine(fullTempPath, ".gitignore")))
            {
                pipeline.Log(LogLevel.Information, $"Creating .gitignore for temp folder.");
                CreateGitIgnore();
            }

            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<string> copiedAssemblies = new List<string>();
            foreach (Assembly assembly in allAssemblies)
            {
                try
                {
                    var assemblyName = Path.GetFileName(assembly.Location);
                    var destPath = Path.Combine(fullTempPath, assemblyName);
                    FileInfo info = new FileInfo(assembly.Location);
                    info.CopyTo(destPath, true);
                    copiedAssemblies.Add($"* Copied {info.Name} from ***{assembly.Location}*** to ***{destPath}***");
                }
                catch (Exception ex) { pipeline.Log(LogLevel.Error, ex.ToString()); }
            }
            pipeline.Log(LogLevel.Information, $"Copied a total of {copiedAssemblies.Count} to the temporary folder for arg4", copiedAssemblies.ToArray());
            return fullTempPath;

            void CreateGitIgnore()
            {
                using (var fileStream = File.Create(Path.Combine(fullTempPath, ".gitignore")))
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    writer.Write("!.gitignore");
                }
            }
        }

        private string[] GetPathOfAssemblyToWeave(IEnumerable<(AsmDef, AssemblyDefinitions)> deserializedAsmDefs, string resolvedArtifactPath, Pipeline pipeline)
        {
            List<string> assemblyPaths = new List<string>();
            foreach (var (asmDef, datum) in deserializedAsmDefs)
            {
                var filesInDirectory = Directory.GetFiles(resolvedArtifactPath);
                string assemblyPath = string.Empty;
                foreach (string path in filesInDirectory)
                {
                    if (Path.GetFileNameWithoutExtension(path) == asmDef.name)
                        assemblyPath = path;
                    break;
                }
                string fullPath = Path.GetFullPath(assemblyPath);
                assemblyPaths.Add(fullPath);
            }
            pipeline.Log(LogLevel.Information, $"Got a total of {assemblyPaths.Count} assemblies to weave as arg3", assemblyPaths.ToArray());
            return assemblyPaths.ToArray();
        }


        private async Task WeaveAssembly(List<string> arguments, Pipeline pipeline)
        {
            var args = new StringBuilder();
            var logger = new List<string>();
            for (int i = 0; i < arguments.Count; i++)
            {
                args.Append($"\"{arguments[i]}\"");
                args.Append(" ");
                logger.Add($"* Argument *{i}*: **\"{arguments[i]}\"¨**");
            }
            var exePath = Path.GetFullPath(PathReference.ResolvePath(WeaverExe, pipeline, this));
            var dirPath = Path.GetFullPath(PathReference.ResolvePath(WeaverDir, pipeline, this));

            var psi = new ProcessStartInfo(exePath)
            {
                WorkingDirectory = dirPath,
                Arguments = args.ToString(),
                UseShellExecute = true
            };

            pipeline.Log(LogLevel.Information, $"Executing {exePath} in directory {dirPath} with the following arguments", logger.ToArray());

            var process = Process.Start(psi);
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (sender, dataArgs) =>
            {
                pipeline.Log(LogLevel.Information, $"{sender} - {dataArgs.Data}");
            };
            while (!process.HasExited)
            {
                await Task.Delay(100);
            }
        }
    }
}
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

        [PathReferenceResolver, Tooltip("Location where built assemblies will be cached before being staged")]
        public string assemblyArtifactPath = "<AssemblyStaging>";
        [HideInInspector]
        public string UNetAssembly = string.Empty;

        public static string WeaverDir = "<WeaverDir>";
        public static string WeaverExe = "<WeaverExe>";
        public static string WeaverTemp = "<WeaverTemp>";

        public override Task Execute(Pipeline pipeline)
        {
            var resolvedArtifactPath = PathReference.ResolvePath(assemblyArtifactPath, pipeline, this);

            var definitionDatums = pipeline.Manifest.Data.OfType<AssemblyDefinitions>().ToArray();
            if (!definitionDatums.Any())
            {
                var scriptPath = UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
                pipeline.Log(LogLevel.Warning, $"No AssemblyDefinitions found, skipping.");
                return Task.CompletedTask;
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
                string unityEngineArg = GetCoreDLL();
                string uNetDLLArg = GetUNetAssemblyPath();
                string outputArg = Path.GetFullPath(resolvedArtifactPath);
                string extraAssemblyArg = GetExtraAssemblies(pipeline);
                string[] assemblyToWeaveArgs = GetPathOfAssemblyToWeave(deserializedAsmDefs, resolvedArtifactPath);

                foreach (string assemblyPath in assemblyToWeaveArgs)
                {
                    List<string> args = new List<string> { unityEngineArg, uNetDLLArg, outputArg, assemblyPath, extraAssemblyArg };
                    WeaveAssembly(args, pipeline);
                }
            }
            finally
            {
                string tempPath = PathReference.ResolvePath(WeaverTemp, pipeline, this);
                if (Directory.Exists(Path.GetFullPath(tempPath)))
                {
                    Directory.Delete(Path.GetFullPath(tempPath), true);
                }
            }

            return Task.CompletedTask;
        }
        private string GetCoreDLL()
        {
            string dataFolder = EditorApplication.applicationContentsPath;
            string managedFolder = Path.Combine(dataFolder, "Managed");
            if (!Directory.Exists(managedFolder))
                throw new Exception();

            string coreModulePath = Path.Combine(managedFolder, "UnityEngine.dll");

            if (!File.Exists(Path.GetFullPath(coreModulePath)))
                throw new Exception();

            string fullPath = Path.GetFullPath(coreModulePath);
            return fullPath;
        }

        private string[] GetPathOfAssemblyToWeave(IEnumerable<(AsmDef, AssemblyDefinitions)> deserializedAsmDefs, string resolvedArtifactPath)
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
            return assemblyPaths.ToArray();
        }

        private string GetUNetAssemblyPath()
        {
            if (UNetAssembly != string.Empty)
                return UNetAssembly;

            Assembly networkingAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetName().Name == "com.unity.multiplayer-hlapi.Runtime")
                .FirstOrDefault();

            if (networkingAssembly == null)
                throw new NullReferenceException();

            UNetAssembly = networkingAssembly.Location;
            Debug.Log(UNetAssembly);
            return UNetAssembly;
        }

        private void WeaveAssembly(List<string> arguments, Pipeline pipeline)
        {
            var args = new StringBuilder();
            for (int i = 0; i < arguments.Count; i++)
            {
                pipeline.Log(LogLevel.Information, $"Arg {i}: \"{arguments[i]}\"");
                args.Append($"\"{arguments[i]}\"");
                args.Append(" ");
            }
            var exePath = Path.GetFullPath(PathReference.ResolvePath(WeaverExe, pipeline, this));
            var dirPath = Path.GetFullPath(PathReference.ResolvePath(WeaverDir, pipeline, this));

            var psi = new ProcessStartInfo(exePath)
            {
                WorkingDirectory = dirPath,
                Arguments = args.ToString(),
                UseShellExecute = true
            };

            pipeline.Log(LogLevel.Information, $"Executing {exePath} in directory {dirPath}");

            Process.Start(psi);
            return;
        }

        //Absolutely cursed.
        private string GetExtraAssemblies(Pipeline pipeline)
        {
            string tempPath = PathReference.ResolvePath(WeaverTemp, pipeline, this);
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            string fullTempPath = Path.GetFullPath(tempPath);
            if (!File.Exists(Path.Combine(fullTempPath, ".gitignore")))
            {
                CreateGitIgnore(fullTempPath);
            }

            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in allAssemblies)
            {
                try
                {
                    var assemblyName = Path.GetFileName(assembly.Location);
                    var destPath = Path.Combine(fullTempPath, assemblyName);
                    if (!File.Exists(destPath))
                    {
                        FileInfo info = new FileInfo(assembly.Location);
                        info.CopyTo(destPath, true);
                    }
                }
                catch (Exception ex) { pipeline.Log(LogLevel.Error, ex.ToString()); }
            }
            return fullTempPath;
        }

        private void CreateGitIgnore(string fullTempPath)
        {
            using (var fileStream = File.Create(Path.Combine(fullTempPath, ".gitignore")))
            using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
            {
                writer.Write("!.gitignore");
            }
        }
    }
}
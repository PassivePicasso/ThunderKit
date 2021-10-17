using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Paths;
using UnityEditor;
using System.Threading.Tasks;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor]
    public class StageAssemblies : PipelineJob
    {
        public static readonly HashSet<AssemblyBuilder> BuildStatus = new HashSet<AssemblyBuilder>();
        static string Combine(params string[] component) => Path.Combine(component).Replace('\\', '/');
#pragma warning disable CS0649 

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

        struct AsmBuildData : IEquatable<AsmBuildData>
        {
            public string Directory;
            public string AssemblyDefinition;
            public string Output;
            public string[] Scripts;

            public override bool Equals(object obj)
            {
                return obj is AsmBuildData data && Equals(data);
            }

            public bool Equals(AsmBuildData other)
            {
                return Directory == other.Directory &&
                       AssemblyDefinition == other.AssemblyDefinition;
            }

            public override int GetHashCode()
            {
                int hashCode = -815763736;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Directory);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AssemblyDefinition);
                return hashCode;
            }

            public static bool operator ==(AsmBuildData left, AsmBuildData right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(AsmBuildData left, AsmBuildData right)
            {
                return !(left == right);
            }
        }
#pragma warning restore CS0649 

        public bool stageDebugDatabases;
        [PathReferenceResolver, Tooltip("Location where built assemblies will be cached before being staged")]
        public string assemblyArtifactPath = "<AssemblyStaging>";
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;


        public sealed override async Task Execute(Pipeline pipeline)
        {
            var resolvedArtifactPath = PathReference.ResolvePath(assemblyArtifactPath, pipeline, this);
            Directory.CreateDirectory(resolvedArtifactPath);

            var assemblies = CompilationPipeline.GetAssemblies();
            var definitionDatums = pipeline.Manifest.Data.OfType<AssemblyDefinitions>().ToArray();
            var deserializedAsmDefs = definitionDatums.SelectMany(datum =>
                datum.definitions.Select(asmDefAsset =>
                    (asmDef: JsonUtility.FromJson<AsmDef>(asmDefAsset.text),
                     asmDefAsset: asmDefAsset,
                     datum: datum)
                ));

            var definitions = deserializedAsmDefs.Select(dataSet =>
                    (asm: assemblies.FirstOrDefault(asm => dataSet.asmDef.name == asm.name),
                     asmDefAsset: dataSet.asmDefAsset,
                     asmDef: dataSet.asmDef,
                     datum: dataSet.datum )
                ).Where(def => def.asm != null)
                .ToArray();

            foreach (var definition in definitions)
            {
                var assemblyName = $"{definition.asm.name}.dll";
                var targetName = Path.GetFileNameWithoutExtension(definition.asm.name);
                var assemblyOutputPath = Combine(resolvedArtifactPath, assemblyName);
                var builder = new AssemblyBuilder(assemblyOutputPath, definition.asm.sourceFiles)
                {
                    additionalReferences = definition.asm.allReferences,
                };
                builder.excludeReferences = builder.defaultReferences.Where(rf => rf.Contains(assemblyName)).ToArray();
                builder.buildTargetGroup = buildTargetGroup;

                var index = pipeline.ManifestIndex;
                void OnBuildStarted(string path) => Debug.Log($"Building : {path}");
                void OnBuildFinished(string path, CompilerMessage[] messages)
                {
                    BuildStatus.Remove(builder);
                    if (messages.Any())
                        foreach (var message in messages.OrderBy(msg => msg.type))
                        {
                            switch (message.type)
                            {
                                case CompilerMessageType.Error:
                                    Debug.LogError(message.message);
                                    break;
                                case CompilerMessageType.Warning:
                                    Debug.LogWarning(message.message);
                                    break;
                            }
                        }
                    else
                        Debug.Log($"Build Completed: {path}");

                    Debug.Log($"Resolving Paths: {path}");
                    var prevIndex = pipeline.ManifestIndex;
                    pipeline.ManifestIndex = index;
                    var resolvedPaths = definition.datum.StagingPaths
                        .Select(p => PathReference.ResolvePath(p, pipeline, this)).ToArray();
                    pipeline.ManifestIndex = prevIndex;
                    Debug.Log($"Resolved Paths: {path}");


                    foreach (var outputPath in resolvedPaths)
                    {

                        Debug.Log($"Copying {assemblyName} to {outputPath}");
                        Directory.CreateDirectory(outputPath);
                        if (stageDebugDatabases)
                            CopyFiles(resolvedArtifactPath, outputPath, $"{targetName}*.pdb", $"{targetName}*.mdb", assemblyName);
                        else
                            CopyFiles(resolvedArtifactPath, outputPath, assemblyName);
                    }
                }
                builder.buildTarget = buildTarget;
                builder.buildFinished += OnBuildFinished;
                builder.buildStarted += OnBuildStarted;

                if (File.Exists(assemblyOutputPath))
                    File.Delete(assemblyOutputPath);
                BuildStatus.Add(builder);
                if (builder.Build())
                {
                    while (EditorApplication.isCompiling)
                    {
                        await Task.Delay(100);
                    }
                }
            }
        }
        void CopyFiles(string sourcePath, string outputPath, params string[] patterns)
        {
            Directory.CreateDirectory(outputPath);
            var targetFiles = (from pattern in patterns
                               from file in Directory.GetFiles(sourcePath, pattern, SearchOption.AllDirectories)
                               select file).ToArray();
            foreach (var source in targetFiles)
            {
                var fileName = Path.GetFileName(source);
                string destination = Combine(outputPath, fileName);
                File.Copy(source, destination, true);
                Debug.Log($"Copied {source} to {destination}");

            }
        }

    }
}
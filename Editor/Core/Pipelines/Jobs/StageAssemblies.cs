using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Paths;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor]
    public class StageAssemblies : PipelineJob
    {
        public bool stageDebugDatabases;
        public BuildTargetGroup Group;
        public ScriptCompilationOptions ScriptCompilationOptions;
        public BuildTarget Target;

        public override void Execute(Pipeline pipeline)
        {
            foreach (var asmDefs in pipeline.Manifest.Data.OfType<AssemblyDefinitions>())
                foreach (var outputPath in asmDefs.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                    CopyReferences(asmDefs.definitions, outputPath);
        }

        void CopyReferences(AssemblyDefinitionAsset[] assemblyDefs, string outputPath)
        {
            var scriptAssemblies = Path.Combine("Library", "ScriptAssemblies");
            var playerScriptAssemblies = Path.Combine("Library", "PlayerScriptAssemblies");

            var buildPath = (ScriptCompilationOptions & ScriptCompilationOptions.DevelopmentBuild) == ScriptCompilationOptions.DevelopmentBuild
                          ? scriptAssemblies
                          : playerScriptAssemblies;

            PlayerBuildInterface.CompilePlayerScripts(new ScriptCompilationSettings
            {
                group = Group,
                options = ScriptCompilationOptions,
                target = Target
            }, buildPath);

            foreach (var definition in assemblyDefs)
            {
                var assemblyDef = JsonUtility.FromJson<AssemblyDef>(definition.text);
                var fileOutputBase = Path.Combine(outputPath, assemblyDef.name);
                var fileName = Path.GetFileName(fileOutputBase);
                Directory.CreateDirectory(outputPath);
                CopyFiles(buildPath, outputPath, $"{fileName}*.dll");

                if (stageDebugDatabases)
                    CopyFiles(buildPath, outputPath, $"{fileName}*.pdb", $"{fileName}*.mdb", $"{fileName}*.dll");
            }
        }


        void CopyFiles(string path, string outputPath, params string[] patterns)
        {
            Directory.CreateDirectory(outputPath);
            var targetFiles = from pattern in patterns
                              from file in Directory.GetFiles(path, pattern, SearchOption.AllDirectories)
                              select file;
            foreach (var file in targetFiles)
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(outputPath, fileName), true);
            }
        }
    }
}
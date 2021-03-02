using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Paths;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline)), ManifestProcessor]
    public class StageAssemblies : PipelineJob
    {
        public bool stageDebugDatabases;

        public override void Execute(Pipeline pipeline)
        {
            foreach (var asmDefs in pipeline.Manifest.Data.OfType<AssemblyDefinitions>())
                foreach (var outputPath in asmDefs.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                    CopyReferences(asmDefs.definitions, outputPath);
        }

        void CopyReferences(AssemblyDefinitionAsset[] assemblyDefs, string ouptputPath)
        {
            var scriptAssemblies = Path.Combine("Library", "ScriptAssemblies");
            var playerScriptAssemblies = Path.Combine("Library", "PlayerScriptAssemblies");

            foreach (var definition in assemblyDefs)
            {
                var assemblyDef = JsonUtility.FromJson<AssemblyDef>(definition.text);
                var fileOutputBase = Path.Combine(ouptputPath, assemblyDef.name);
                var fileName = Path.GetFileName(fileOutputBase);
                Directory.CreateDirectory(Path.GetDirectoryName(fileOutputBase));

                if (File.Exists($"{fileOutputBase}.dll")) File.Delete($"{fileOutputBase}.dll");
                var playerBuildExists = File.Exists(Path.Combine(playerScriptAssemblies, $"{assemblyDef.name}.dll"));
                if (playerBuildExists)
                    File.Copy(Path.Combine(playerScriptAssemblies, $"{assemblyDef.name}.dll"), $"{fileOutputBase}.dll");
                else
                    File.Copy(Path.Combine(scriptAssemblies, $"{assemblyDef.name}.dll"), $"{fileOutputBase}.dll");

                if (stageDebugDatabases)
                {
                    if (File.Exists($"{fileOutputBase}.dll.mdb")) File.Delete($"{fileOutputBase}.dll.mdb");
                    if (File.Exists($"{fileOutputBase}.pdb")) File.Delete($"{fileOutputBase}.pdb");

                    string pdbPath = Path.Combine(scriptAssemblies, $"{fileName}.pdb");
                    string playerPdbPath = Path.Combine(playerScriptAssemblies, $"{fileName}.pdb");
                    string mdbPath = $"{Path.Combine(scriptAssemblies, fileName)}.dll.mdb";
                    string playerMdbPath = $"{Path.Combine(playerScriptAssemblies, fileName)}.dll.mdb";

                    if (File.Exists(playerMdbPath)) File.Copy($"{Path.Combine(playerScriptAssemblies, fileName)}.dll.mdb", $"{fileOutputBase}.dll.mdb");
                    else if (File.Exists(mdbPath)) File.Copy(mdbPath, $"{fileOutputBase}.dll.mdb");

                    if (File.Exists(playerPdbPath)) File.Copy($"{Path.Combine(playerScriptAssemblies, fileName)}.dll.pdb", $"{fileOutputBase}.dll.pdb");
                    else if (File.Exists(pdbPath)) File.Copy(pdbPath, $"{fileOutputBase}.dll.pdb");
                }
            }
        }
    }
}
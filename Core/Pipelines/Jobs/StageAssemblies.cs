using System.IO;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests.Common;
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
            foreach (var manifest in pipeline.manifests)
                foreach (var asmDefs in pipeline.Datums.OfType<AssemblyDefinitions>())
                {
                    var outputPath = asmDefs.output.GetPath(pipeline);
                    CopyReferences(asmDefs.definitions, outputPath);
                }
        }
        
        void CopyReferences(AssemblyDefinitionAsset[] assemblyDefs, string ouptputPath)
        {
            var scriptAssemblies = Path.Combine("Library", "ScriptAssemblies");

            foreach (var definition in assemblyDefs)
            {
                var assemblyDef = JsonUtility.FromJson<AssemblyDef>(definition.text);
                var fileOutputBase = Path.Combine(ouptputPath, assemblyDef.name);
                var fileName = Path.GetFileName(fileOutputBase);
                Directory.CreateDirectory(Path.GetDirectoryName(fileOutputBase));

                if (File.Exists($"{fileOutputBase}.dll")) File.Delete($"{fileOutputBase}.dll");
                File.Copy(Path.Combine(scriptAssemblies, $"{assemblyDef.name}.dll"), $"{fileOutputBase}.dll");

                if (stageDebugDatabases)
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
    }
}
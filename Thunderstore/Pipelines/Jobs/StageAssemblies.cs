#if UNITY_EDITOR
using ThunderKit.Core.Data;
using ThunderKit.Core.Pipelines;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderKit.Thunderstore.Pipelines.Steps
{

    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class StageAssemblies : PipelineJob
    {
        public bool stageDebugDatabases;

        public override void Execute(Pipeline pipeline)
        {
            var manifestPipeline = (pipeline as ManifestPipeline);
            var manifest = manifestPipeline.Manifest;

            CopyReferences(manifest.plugins, manifestPipeline.PluginStagingPath);
            CopyReferences(manifest.patchers, manifestPipeline.PatchersStagingPath);
            CopyReferences(manifest.monomod, manifestPipeline.MonoModStagingPath);

            var manifestJson = manifest.RenderJson();
            if (Directory.Exists(manifestPipeline.PluginStagingPath)) File.WriteAllText(Path.Combine(manifestPipeline.PluginStagingPath, "manifest.json"), manifestJson);

            var settings = ThunderKitSettings.GetOrCreateSettings();
            if (settings?.deployment_exclusions?.Any() ?? false)
                foreach (var deployment_exclusion in settings.deployment_exclusions)
                    foreach (var file in Directory.EnumerateFiles(manifestPipeline.PluginStagingPath, deployment_exclusion, SearchOption.AllDirectories).ToArray())
                        File.Delete(file);
        }

        void CopyReferences(AssemblyDefinitionAsset[] assemblyDefs, string ouptputPath)
        {
            var scriptAssemblies = Path.Combine("Library", "ScriptAssemblies");

            foreach (var plugin in assemblyDefs)
            {
                var assemblyDef = JsonUtility.FromJson<AssemblyDef>(plugin.text);
                var fileOutputBase = Path.Combine(ouptputPath, assemblyDef.name);
                var fileName = Path.GetFileName(fileOutputBase);

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
#endif
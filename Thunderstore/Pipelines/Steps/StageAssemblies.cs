#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Editor;
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{

    [PipelineSupport(typeof(ManifestPipeline)), ManifestProcessor]
    public class StageAssemblies: PipelineJob
    {
        public bool stageDebugDatabases;

        public override void Execute(Pipeline pipeline)
        {
            var manifestPipeline = (pipeline as ManifestPipeline);
            var manifest = manifestPipeline.Manifest;

            CopyReferences(manifest.plugins, manifestPipeline.PluginsPath);
            CopyReferences(manifest.patchers, manifestPipeline.PatchersPath);
            CopyReferences(manifest.monomod, manifestPipeline.MonomodPath);

            var manifestJson = manifest.RenderJson();
            if (Directory.Exists(manifestPipeline.PluginsPath)) File.WriteAllText(Path.Combine(manifestPipeline.PluginsPath, "manifest.json"), manifestJson);

            var settings = ThunderKitSettings.GetOrCreateSettings();
            if (settings?.deployment_exclusions?.Any() ?? false)
                foreach (var deployment_exclusion in settings.deployment_exclusions)
                    foreach (var file in Directory.EnumerateFiles(manifestPipeline.PluginsPath, deployment_exclusion, SearchOption.AllDirectories).ToArray())
                        File.Delete(file);
        }

        void CopyReferences(AssemblyDefinitionAsset[] assemblyDefs, string assemblyOutputPath)
        {
            var scriptAssemblies = Path.Combine("Library", "ScriptAssemblies");

            foreach (var plugin in assemblyDefs)
            {
                var assemblyDef = JsonUtility.FromJson<AssemblyDef>(plugin.text);
                var fileOutputBase = Path.Combine(assemblyOutputPath, assemblyDef.name);
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
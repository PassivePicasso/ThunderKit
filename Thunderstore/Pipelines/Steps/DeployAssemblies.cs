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
    public class DeployAssemblies : PipelineJob
    {
        public bool deployMdbs;
        public override void Execute(Pipeline pipeline)
        {
            var outputRoot/*   */= Path.Combine(pipeline.OutputRoot, pipeline.name);
            var bepinexDir/*     */= Path.Combine(outputRoot, "BepInExPack", "BepInEx");

            foreach (var manifest in (pipeline as ManifestPipeline).manifests)
                CopyAllReferences(bepinexDir, manifest);
        }

        void CopyAllReferences(string outputRoot, Manifest manifest)
        {
            var pluginPath = Path.Combine(outputRoot, "plugins", manifest.name);
            var patcherPath = Path.Combine(outputRoot, "patchers", manifest.name);
            var monomodPath = Path.Combine(outputRoot, "monomod", manifest.name);

            if (manifest.plugins.Any() && !Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);
            if (manifest.patchers.Any() && !Directory.Exists(patcherPath)) Directory.CreateDirectory(patcherPath);
            if (manifest.monomod.Any() && !Directory.Exists(monomodPath)) Directory.CreateDirectory(monomodPath);

            CopyReferences(manifest.plugins, pluginPath);
            CopyReferences(manifest.patchers, patcherPath);
            CopyReferences(manifest.monomod, monomodPath);

            var manifestJson = manifest.RenderJson();
            if (Directory.Exists(pluginPath)) File.WriteAllText(Path.Combine(pluginPath, "manifest.json"), manifestJson);

            var settings = ThunderKitSettings.GetOrCreateSettings();
            if (settings?.deployment_exclusions?.Any() ?? false)
                foreach (var deployment_exclusion in settings.deployment_exclusions)
                    foreach (var file in Directory.EnumerateFiles(pluginPath, deployment_exclusion, SearchOption.AllDirectories).ToArray())
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

                if (deployMdbs)
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
#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Editor;
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline))]
    public class PackageManifest : PipelineJob
    {

        public override void Execute(Pipeline pipeline)
        {
            var manifest = (pipeline as ManifestPipeline).Manifest;
            var output/*   */= Path.Combine(pipeline.OutputRoot, pipeline.name);
            var bepinexDir/*     */= Path.Combine(output, "BepInExPack", "BepInEx");
            var deployments/*    */= "Deployments";
            var outputPath/*     */= Path.Combine(deployments, manifest.name);

            CopyAllReferences(outputPath, manifest);

            if (manifest.readme)
            {
                var readmePath = AssetDatabase.GetAssetPath(manifest.readme);
                File.Copy(readmePath, Path.Combine(outputPath, "README.md"), true);
            }
            else File.WriteAllText(Path.Combine(outputPath, "README.md"), $"# {manifest.name}");


            if (manifest.icon)
                File.WriteAllBytes(Path.Combine(outputPath, "icon.png"), manifest.icon.EncodeToPNG());

            string outputFile = Path.Combine(deployments, $"{manifest.name}.zip");
            if (File.Exists(outputFile)) File.Delete(outputFile);

            ZipFile.CreateFromDirectory(outputPath, outputFile);
        }

        void SimpleCopy(string outputRoot, Manifest manifest)
        {

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

            File.WriteAllText(Path.Combine(outputRoot, "manifest.json"), manifestJson);

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

                if (File.Exists($"{fileOutputBase}.dll")) File.Delete($"{fileOutputBase}.dll");
                File.Copy(Path.Combine(scriptAssemblies, $"{assemblyDef.name}.dll"), $"{fileOutputBase}.dll");
            }
        }
    }
}
#endif
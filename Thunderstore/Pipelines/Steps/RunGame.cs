#if UNITY_EDITOR

using PassivePicasso.ThunderKit.Deploy.Editor;
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline))]
    public class RunGame : PipelineJob
    {
        public bool deployAssemblies;
        public bool deployMdbs;
        public string[] extraCommandLineArgs;

        public override void Execute(Pipeline pipeline)
        {
            var manifest = (pipeline as ManifestPipeline).Manifest;
            var settings/*       */= ThunderKitSettings.GetOrCreateSettings();
            var ror2Executable = Path.Combine(settings.GamePath, settings.GameExecutable);
            var outputRoot/*   */= Path.Combine(pipeline.OutputRoot, pipeline.name);
            var bepinexDir/*     */= Path.Combine(outputRoot, "BepInExPack", "BepInEx");
            var bepinexCoreDir/* */= Path.Combine(bepinexDir, "core");
            if (File.Exists(Path.Combine(settings.GamePath, "doorstop_config.ini")))
                File.Move(Path.Combine(settings.GamePath, "doorstop_config.ini"), Path.Combine(settings.GamePath, "doorstop_config.bak.ini"));

            CopyAllReferences(bepinexDir, manifest);
            UnityEngine.Debug.Log($"Launching {Path.GetFileNameWithoutExtension(settings.GameExecutable)}");
            var arguments = new List<string>
                            {
                                "--doorstop-enable true",
                                $"--doorstop-target \"{Path.Combine(Directory.GetCurrentDirectory(), bepinexCoreDir, "BepInEx.Preloader.dll")}\""
                            };
            if (extraCommandLineArgs?.Any() ?? false)
                arguments.AddRange(extraCommandLineArgs);

            var args = arguments.Aggregate((a, b) => $"{a} {b}");

            var rorPsi = new ProcessStartInfo(ror2Executable)
            {
                WorkingDirectory = bepinexDir,
                Arguments = args,

                //Standard output redirection doesn't currently work with bepinex, appears to be considered a bepinex bug
                //RedirectStandardOutput = true,
                UseShellExecute = true
            };

            var rorProcess = new Process { StartInfo = rorPsi, EnableRaisingEvents = true };
            EventHandler RorProcess_Exited = null;
            RorProcess_Exited = new EventHandler((object sender, EventArgs e) =>
            {
                rorProcess.Exited -= RorProcess_Exited;
                if (File.Exists(Path.Combine(settings.GamePath, "doorstop_config.bak.ini")))
                    File.Move(Path.Combine(settings.GamePath, "doorstop_config.bak.ini"), Path.Combine(settings.GamePath, "doorstop_config.ini"));
            });
            rorProcess.Exited += RorProcess_Exited;

            rorProcess.Start();
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

            //if (deployment.DeploymentOptions.HasFlag(DeploymentOptions.Package))
            //    File.WriteAllText(Path.Combine(outputRoot, "manifest.json"), manifestJson);

            var settings = ThunderKitSettings.GetOrCreateSettings();
            if (settings?.deployment_exclusions?.Any() ?? false)
                foreach (var deployment_exclusion in settings.deployment_exclusions)
                    foreach (var file in Directory.EnumerateFiles(pluginPath, deployment_exclusion, SearchOption.AllDirectories).ToArray())
                        File.Delete(file);
        }


        void CopyReferences(AssemblyDefinitionAsset[] assemblyDefs, string assemblyOutputPath)
        {
            var scriptAssemblies = Path.Combine("Library", "ScriptAssemblies");
            if (!deployAssemblies && !deployMdbs) return;

            foreach (var plugin in assemblyDefs)
            {
                var assemblyDef = JsonUtility.FromJson<AssemblyDef>(plugin.text);
                var fileOutputBase = Path.Combine(assemblyOutputPath, assemblyDef.name);
                var fileName = Path.GetFileName(fileOutputBase);

                if (deployAssemblies)
                {
                    if (File.Exists($"{fileOutputBase}.dll")) File.Delete($"{fileOutputBase}.dll");
                    File.Copy(Path.Combine(scriptAssemblies, $"{assemblyDef.name}.dll"), $"{fileOutputBase}.dll");
                }

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
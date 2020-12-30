#if UNITY_EDITOR

using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [PipelineSupport(typeof(ManifestPipeline))]
    public class RunGame : PipelineJob
    {
        public string[] extraCommandLineArgs;

        public override void Execute(Pipeline pipeline)
        {
            var settings/*       */= ThunderKitSettings.GetOrCreateSettings();
            var ror2Executable = Path.Combine(settings.GamePath, settings.GameExecutable);
            var outputRoot/*   */= Path.Combine(pipeline.OutputRoot, pipeline.name);
            var bepinexDir/*     */= Path.Combine(outputRoot, "BepInExPack", "BepInEx");
            var bepinexCoreDir/* */= Path.Combine(bepinexDir, "core");

            if (File.Exists(Path.Combine(settings.GamePath, "doorstop_config.ini")))
                File.Move(Path.Combine(settings.GamePath, "doorstop_config.ini"), Path.Combine(settings.GamePath, "doorstop_config.bak.ini"));

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
    }
}

#endif
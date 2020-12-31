#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Editor;
using PassivePicasso.ThunderKit.Utilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines.Steps
{
    [Flags]
    public enum LogLevel
    {
        //Disables all log messages
        None = 0,
        //Errors which cannot be recovered from; the game cannot continue to run
        Fatal = 1,
        //Errors are recoverable from; the game can be run, albeit with further errors
        Error = 2,
        //Messages that signify an anomaly that is not an error
        Warning = 4,
        //Important messages that should be displayed
        Message = 8,
        //Messages of low importance
        Info = 16,
        //Messages intended for developers
        Debug = 32,

        //All = Fatal | Error | Warning | Message | Info | Debug
    }

    [PipelineSupport(typeof(ManifestPipeline))]
    public class DeployBepinex : PipelineJob
    {
        [EnumFlag]
        public LogLevel LogLevel;

        public bool ShowConsole;
        public bool CleanInstall;

        public override void Execute(Pipeline pipeline) => ExecuteAsync(pipeline);

        private async void ExecuteAsync(Pipeline pipeline)
        {
            var bepinexDir/* */= Path.Combine(pipeline.OutputRoot, "BepInExPack", "BepInEx");

            var bepinexPacks = ThunderLoad.LookupPackage("BepInExPack");
            var bepinex = bepinexPacks.FirstOrDefault();

            var filePath = Path.Combine(pipeline.OutputRoot, $"{bepinex.full_name}.zip");
            if (!File.Exists(filePath))
                await ThunderLoad.DownloadPackageAsync(bepinex, filePath);

            using (var fileStream = File.OpenRead(filePath))
            using (var archive = new ZipArchive(fileStream))
            {
                foreach (var entry in archive.Entries)
                {
                    string outputFile = Path.Combine(pipeline.OutputRoot, entry.FullName);
                    if (File.Exists(outputFile)) File.Delete(outputFile);
                }
                archive.ExtractToDirectory(pipeline.OutputRoot);
            }

            if (File.Exists(Path.Combine(pipeline.OutputRoot, "icon.png"))) File.Delete(Path.Combine(pipeline.OutputRoot, "icon.png"));
            if (File.Exists(Path.Combine(pipeline.OutputRoot, "manifest.json"))) File.Delete(Path.Combine(pipeline.OutputRoot, "manifest.json"));
            if (File.Exists(Path.Combine(pipeline.OutputRoot, "README.md"))) File.Delete(Path.Combine(pipeline.OutputRoot, "README.md"));

            string configPath = Path.Combine(bepinexDir, "Config", "BepInEx.cfg");
            if (Directory.Exists(Path.Combine(bepinexDir, "Config")))
            {
                File.Delete(configPath);
                var logLevels = LogLevel.GetFlags().Select(f => $"{f}").Aggregate((a, b) => $"{a}, {b}");
                string contents = ConfigTemplate.CreatBepInExConfig(ShowConsole, logLevels);
                File.WriteAllText(configPath, contents);
            }
        }
    }
}
#endif
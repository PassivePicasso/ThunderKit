﻿#if UNITY_EDITOR
using ThunderKit.Core.Editor;
using ThunderKit.Core.Pipelines;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace ThunderKit.Thunderstore.Pipelines
{
    public class ManifestPipeline : Pipeline
    {
        [MenuItem(Constants.ThunderStorePath + nameof(ManifestPipeline), false, priority = Core.Constants.ThunderKitMenuPriority)]
        public static void CreateManifestPipeline() => ScriptableHelper.SelectNewAsset<ManifestPipeline>();

        public Manifest[] manifests;
        public int StepIndex { get; private set; }
        public int ManifestIndex { get; private set; }
        public Manifest Manifest => manifests[ManifestIndex];
        public string StagingPath => Path.Combine(OutputRoot, "Staging");
        public string DependenciesPath => Path.Combine("Packages");
        public string ManifestPath => Path.Combine(StagingPath, Manifest.name);
        public string PluginStagingPath => Path.Combine(ManifestPath, "plugins", Manifest.name);
        public string PatchersStagingPath => Path.Combine(ManifestPath, "patchers", Manifest.name);
        public string MonoModStagingPath => Path.Combine(ManifestPath, "monomod", Manifest.name);
        public override void Execute()
        {
            for (ManifestIndex = 0; ManifestIndex < manifests.Length; ManifestIndex++)
                if (manifests[ManifestIndex])
                {
                    if (!Directory.Exists(PluginStagingPath)) Directory.CreateDirectory(PluginStagingPath);
                    if (!Directory.Exists(PatchersStagingPath)) Directory.CreateDirectory(PatchersStagingPath);
                    if (!Directory.Exists(MonoModStagingPath)) Directory.CreateDirectory(MonoModStagingPath);
                }

            var jobs = RunSteps.ToArray();
            for (StepIndex = 0; StepIndex < jobs.Length; StepIndex++)
                if (jobs[StepIndex].GetType().GetCustomAttributes<ManifestProcessorAttribute>().Any())
                {
                    for (ManifestIndex = 0; ManifestIndex < manifests.Length; ManifestIndex++)
                        if (manifests[ManifestIndex])
                            jobs[StepIndex].Execute(this);
                }
                else
                    jobs[StepIndex].Execute(this);
        }
    }
}
#endif
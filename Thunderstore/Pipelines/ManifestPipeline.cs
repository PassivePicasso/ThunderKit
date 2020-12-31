#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines
{
    public class ManifestPipeline : Pipeline
    {
        [MenuItem(ScriptableHelper.ThunderKitContextRoot + nameof(ManifestPipeline), false)]
        public static void CreateManifestPipeline() => ScriptableHelper.SelectNewAsset<ManifestPipeline>();

        public Manifest[] manifests;
        public int StepIndex { get; private set; }
        public int ManifestIndex { get; private set; }
        public Manifest Manifest => manifests[ManifestIndex];
        public string StagingPath => Path.Combine(OutputRoot, "Staging");
        public string ManifestPath => Path.Combine(StagingPath, Manifest.name);
        public string PluginStagingPath => Path.Combine(ManifestPath, "plugins", Manifest.name);
        public string PatchersStagingPath => Path.Combine(ManifestPath, "patchers", Manifest.name);
        public string MonoModStagingPath => Path.Combine(ManifestPath, "monomod", Manifest.name);
        public override void Execute()
        {
            for (ManifestIndex = 0; ManifestIndex < manifests.Length; ManifestIndex++)
                if (manifests[ManifestIndex])
                {
                    if (Manifest.plugins.Any() && !Directory.Exists(PluginStagingPath)) Directory.CreateDirectory(PluginStagingPath);
                    if (Manifest.patchers.Any() && !Directory.Exists(PatchersStagingPath)) Directory.CreateDirectory(PatchersStagingPath);
                    if (Manifest.monomod.Any() && !Directory.Exists(MonoModStagingPath)) Directory.CreateDirectory(MonoModStagingPath);
                }

            for (StepIndex = 0; StepIndex < runSteps.Length; StepIndex++)
                if (runSteps[StepIndex].GetType().GetCustomAttributes<ManifestProcessorAttribute>().Any())
                {
                    for (ManifestIndex = 0; ManifestIndex < manifests.Length; ManifestIndex++)
                        if (manifests[ManifestIndex])
                            runSteps[StepIndex].Execute(this);
                }
                else
                    runSteps[StepIndex].Execute(this);
        }
    }
}
#endif
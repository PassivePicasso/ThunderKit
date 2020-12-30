#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;
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
        public int Index { get; private set; }
        public Manifest Manifest => manifests[Index];
        public override void Execute()
        {
            for (int stepIndex = 0; stepIndex < runSteps.Length; stepIndex++)
                if (runSteps[stepIndex].GetType().GetCustomAttributes<ManifestProcessorAttribute>().Any())
                    for (Index = 0; Index < manifests.Length; Index++)
                        if (manifests[Index])
                            runSteps[stepIndex].Execute(this);
                else
                    runSteps[stepIndex].Execute(this);
        }
    }
}
#endif
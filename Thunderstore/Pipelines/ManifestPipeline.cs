using PassivePicasso.ThunderKit.Deploy.Pipelines;
using PassivePicasso.ThunderKit.Utilities;
using UnityEditor;

namespace PassivePicasso.ThunderKit.Thunderstore.Pipelines
{
    public class ManifestPipeline : Pipeline
    {
        [MenuItem(ScriptableHelper.ThunderKitContextRoot + nameof(ManifestPipeline), false)]
        public static void CreateManifestPipeline() => ScriptableHelper.SelectNewAsset<ManifestPipeline>();

        public Manifest[] manifests;
        public int Index { get; private set; }
        public Manifest Manifest { get; private set; }
        public override void Execute()
        {
            for (Index = 0; Index < manifests.Length; Index++)
            {
                Manifest = manifests[Index];
                foreach (var step in runSteps) step.Execute(this);
            }
        }
    }
}
using ThunderKit.Core.Manifests;
using UnityEditor;

namespace ThunderKit.Core.Pipelines.PathComponents
{
    public class AssetReference : PathComponent
    {
        public DefaultAsset Asset;
        public override string GetPath(PathReference output, Manifest manifest, Pipeline pipeline) => AssetDatabase.GetAssetPath(Asset);
    }
}

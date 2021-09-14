using ThunderKit.Core.Manifests;
using ThunderKit.Core.Pipelines;
using UnityEditor;

namespace ThunderKit.Core.Paths.Components
{
    public class AssetReference : PathComponent
    {
        public DefaultAsset Asset;
        protected override string GetPathInternal(PathReference output, Pipeline pipeline) => AssetDatabase.GetAssetPath(Asset);
    }
}

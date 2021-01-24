using ThunderKit.Core.Manifests;
using ThunderKit.Core.Pipelines;
using UnityEditor;

namespace ThunderKit.Core.Paths.Components
{
    public class MainAssetName : PathComponent
    {
        public override string GetPath(PathReference output, Pipeline pipeline)
        {
            var path = AssetDatabase.GetAssetPath(this);
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            return mainAsset.name;
        }
    }
}

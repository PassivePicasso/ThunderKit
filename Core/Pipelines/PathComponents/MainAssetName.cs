
using PassivePicasso.ThunderKit.Core.Pipelines;
using PassivePicasso.ThunderKit.Core.Manifests;
using UnityEditor;

namespace ThunderKit.Core.Pipelines.PathComponents
{
    public class MainAssetName : PathComponent
    {
        public override string GetPath(PathReference output, Manifest manifest, Pipeline pipeline)
        {
            var path = AssetDatabase.GetAssetPath(this);
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            return mainAsset.name;
        }
    }
}

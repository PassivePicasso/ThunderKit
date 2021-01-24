using UnityEngine;

namespace PassivePicasso.ThunderKit.Core.Pipelines
{
    public class PathComponent : ScriptableObject
    {
        public virtual string GetPath(PathReference output, Manifests.Manifest manifest, Pipeline pipeline) => "";
    }
}
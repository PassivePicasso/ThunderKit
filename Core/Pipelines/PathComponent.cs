using UnityEngine;

namespace ThunderKit.Core.Pipelines
{
    public class PathComponent : ScriptableObject
    {
        public virtual string GetPath(PathReference output, Manifests.Manifest manifest, Pipeline pipeline) => "";
    }
}
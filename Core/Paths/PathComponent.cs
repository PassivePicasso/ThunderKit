using ThunderKit.Core.Pipelines;
using UnityEngine;

namespace ThunderKit.Core.Paths
{
    public class PathComponent : ScriptableObject
    {
        public virtual string GetPath(PathReference output, Pipeline pipeline) => "";
    }
}
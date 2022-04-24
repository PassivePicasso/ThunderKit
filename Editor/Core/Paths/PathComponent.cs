using ThunderKit.Core.Pipelines;

namespace ThunderKit.Core.Paths
{
    public class PathComponent : ComposableElement
    {
        public string GetPath(PathReference output, Pipeline pipeline) => GetPathInternal(output, pipeline);

        protected virtual string GetPathInternal(PathReference output, Pipeline pipeline) => "";
    }
}
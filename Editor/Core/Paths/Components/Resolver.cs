using ThunderKit.Core.Pipelines;

namespace ThunderKit.Core.Paths.Components
{
    public class Resolver : PathComponent
    {
        public string value;
        protected override string GetPathInternal(PathReference output, Pipeline pipeline) => value.Resolve(pipeline, this);
    }
}

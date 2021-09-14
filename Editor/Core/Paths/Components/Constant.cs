using ThunderKit.Core.Pipelines;

namespace ThunderKit.Core.Paths.Components
{
    public class Constant : PathComponent
    {
        public string Value;
        protected override string GetPathInternal(PathReference output, Pipeline pipeline) => Value;
    }
}

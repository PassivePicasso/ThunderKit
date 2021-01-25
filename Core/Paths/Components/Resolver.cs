using ThunderKit.Core.Attributes;
using ThunderKit.Core.Pipelines;

namespace ThunderKit.Core.Paths.Components
{
    public class Resolver : PathComponent
    {
        [PathReferenceResolver]
        public string value;
        public override string GetPath(PathReference output, Pipeline pipeline) => value.Resolve(pipeline, this);
    }
}

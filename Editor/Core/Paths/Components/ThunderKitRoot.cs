using ThunderKit.Core.Manifests;
using ThunderKit.Core.Pipelines;

namespace ThunderKit.Core.Paths.Components
{
    public class ThunderKitRoot : PathComponent
    {
        protected override string GetPathInternal(PathReference output, Pipeline pipeline) => "ThunderKit";
    }
}

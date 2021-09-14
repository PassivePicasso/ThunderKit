using ThunderKit.Core.Manifests;
using ThunderKit.Core.Pipelines;

namespace ThunderKit.Core.Paths.Components
{
    public class WorkingDirectory : PathComponent
    {
        protected override string GetPathInternal(PathReference output, Pipeline pipeline) => System.IO.Directory.GetCurrentDirectory();
    }
}

using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Paths.Components
{
    public class ManifestVersion : PathComponent
    {
        protected override string GetPathInternal(PathReference output, Pipeline pipeline)
        {
            return pipeline.Manifest.Identity.Version;
        }
    }
}

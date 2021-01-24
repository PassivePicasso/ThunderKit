
using PassivePicasso.ThunderKit.Core.Pipelines;
using PassivePicasso.ThunderKit.Core.Manifests;

namespace ThunderKit.Core.Pipelines.PathComponents
{
    public class ManifestName : PathComponent
    {
        public override string GetPath(PathReference output, Manifest manifest, Pipeline pipeline)
        {
            return manifest.name;
        }
    }
}

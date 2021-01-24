using PassivePicasso.ThunderKit.Core.Manifests;
using PassivePicasso.ThunderKit.Core.Pipelines;

namespace ThunderKit.Core.Pipelines.PathComponents
{
    public class OutputReference : PathComponent
    {
        public PathReference reference;
        public override string GetPath(PathReference output, Manifest manifest, Pipeline pipeline) => reference.GetPath(manifest, pipeline);
    }
}

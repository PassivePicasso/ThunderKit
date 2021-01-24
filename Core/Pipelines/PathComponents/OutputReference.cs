using ThunderKit.Core.Manifests;

namespace ThunderKit.Core.Pipelines.PathComponents
{
    public class OutputReference : PathComponent
    {
        public PathReference reference;
        public override string GetPath(PathReference output, Manifest manifest, Pipeline pipeline) => reference.GetPath(manifest, pipeline);
    }
}

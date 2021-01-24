
using PassivePicasso.ThunderKit.Core.Pipelines;
using PassivePicasso.ThunderKit.Core.Manifests;

namespace ThunderKit.Core.Pipelines.PathComponents
{
    public class WorkingDirectory : PathComponent
    {
        public override string GetPath(PathReference output, Manifest manifest, Pipeline pipeline) => System.IO.Directory.GetCurrentDirectory();
    }
}

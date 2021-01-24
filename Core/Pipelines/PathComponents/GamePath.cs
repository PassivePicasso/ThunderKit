
using PassivePicasso.ThunderKit.Core.Pipelines;
using PassivePicasso.ThunderKit.Core.Manifests;
using PassivePicasso.ThunderKit.Core.Data;

namespace ThunderKit.Core.Pipelines.PathComponents
{
    public class GamePath : PathComponent
    {
        public override string GetPath(PathReference output, Manifest manifest, Pipeline pipeline) => ThunderKitSettings.GetOrCreateSettings().GamePath;
    }
}

using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests;

namespace ThunderKit.Core.Pipelines.PathComponents
{
    public class GameExecutable : PathComponent
    {
        public override string GetPath(PathReference output, Manifest manifest, Pipeline pipeline) => ThunderKitSettings.GetOrCreateSettings().GameExecutable;
    }
}

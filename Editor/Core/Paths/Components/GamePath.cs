using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Pipelines;

namespace ThunderKit.Core.Paths.Components
{
    public class GamePath : PathComponent
    {
        protected override string GetPathInternal(PathReference output, Pipeline pipeline) => ThunderKitSettings.GetOrCreateSettings<ThunderKitSettings>().GamePath;
    }
}

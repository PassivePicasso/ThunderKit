using System.IO;
using System.Linq;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;

namespace ThunderKit.BepInEx
{
    public class BepInExPackageDirectory : PathComponent
    {
        public override string GetPath(PathReference output, Pipeline pipeline)
        {
            return Directory.EnumerateDirectories("Packages", "bbepis-BepInExPack*", SearchOption.TopDirectoryOnly).First();
        }
    }
}

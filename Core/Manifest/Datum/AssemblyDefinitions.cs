using ThunderKit.Core.Paths;
using UnityEditorInternal;

namespace ThunderKit.Core.Manifests.Common
{
    public class AssemblyDefinitions : ManifestDatum
    {
        public PathReference output;
        public AssemblyDefinitionAsset[] definitions;
    }
}
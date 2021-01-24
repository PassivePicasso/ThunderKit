using PassivePicasso.ThunderKit.Core.Pipelines;
using UnityEditorInternal;

namespace PassivePicasso.ThunderKit.Core.Manifests.Common
{
    public class AssemblyDefinitions : ManifestDatum
    {
        public PathReference output;
        public AssemblyDefinitionAsset[] definitions;
    }
}
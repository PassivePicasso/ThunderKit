using ThunderKit.Core.Attributes;

namespace ThunderKit.Core.Manifests
{
    public class ManifestDatum : ComposableElement
    {
        [PathReferenceResolver]
        public string[] StagingPaths;
    }
}
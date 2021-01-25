using ThunderKit.Core.Attributes;
using UnityEngine;

namespace ThunderKit.Core.Manifests
{
    public class ManifestDatum : ScriptableObject
    {
        [PathReferenceResolver]
        public string[] StagingPaths;
    }
}
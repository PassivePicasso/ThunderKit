using ThunderKit.Core.Manifests;
using UnityEngine;

namespace ThunderKit.Thunderstore.Manifests
{
    public class ThunderstoreManifest : ManifestDatum
    {
        public string author;
        public string versionNumber;
        public string url;
        public string description;
        public TextAsset readme;
        public Texture2D icon;
        public DependencyList dependencies;
    }
}
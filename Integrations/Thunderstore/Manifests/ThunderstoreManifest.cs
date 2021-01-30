using System.Collections.Generic;
using ThunderKit.Core.Manifests;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore.Manifests
{
    public class ThunderstoreManifest : ManifestDatum
    {
        public string author;
        public string versionNumber;
        public string url;
        public string description;
        public TextAsset readme;
        public Texture2D icon;
        public List<string> dependencies = new List<string>();
    }
}
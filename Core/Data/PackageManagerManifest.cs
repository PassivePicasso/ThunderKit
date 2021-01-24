#if UNITY_EDITOR
using System.Collections.Generic;

namespace PassivePicasso.ThunderKit.Core.Data
{
    public struct PackageManagerManifest
    {
        internal readonly static Dictionary<string, string> EmptyDictionary = new Dictionary<string, string>();

        public string name;
        public string displayName;
        public string version;
        public string unity;
        public string description;
        public Dictionary<string, string> dependencies;

        public PackageManagerManifest(string name, string displayName, string version, string unity, string description)
        {
            this.name = name;
            this.displayName = displayName;
            this.version = version;
            this.unity = unity;
            this.description = description;
            this.dependencies = EmptyDictionary;
        }
    }
}
#endif
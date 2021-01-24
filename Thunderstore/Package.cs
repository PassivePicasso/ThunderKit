#if UNITY_EDITOR
using System;
using System.Linq;

namespace ThunderKit.Thunderstore
{
    [Serializable]
    public partial class Package
    {
        public string name;
        public string full_name;
        public string owner;
        public string version_number;
        public string package_url;
        public DateTime date_created;
        public DateTime date_updated;
        public long rating_score;
        public bool is_pinned;
        public bool is_deprecated;
        public bool has_nsfw_content;
        public string[] categories;
        public string uuid4;
        public VersionedPackage[] versions;
        public VersionedPackage latest
        {
            get
            {
                if (latestPackage != null) return latestPackage;
                latestPackage = versions.OrderByDescending(v => v.version_number).First();
                return latestPackage;
            }
        }

        public VersionedPackage latestPackage { get; private set; }
    }

}
#endif
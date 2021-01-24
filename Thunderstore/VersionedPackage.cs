#if UNITY_EDITOR
using System;
namespace ThunderKit.Thunderstore
{

    [Serializable]
    public partial class VersionedPackage
    {
        public string name;
        public string full_name;
        public string description;
        public string icon;
        public string website_url;
        public Version version_number;
        public string[] dependencies;
        public string download_url;
        public long downloads;
        public DateTime date_created;
        public bool is_active;
        public string uuid4;
    }
}
#endif
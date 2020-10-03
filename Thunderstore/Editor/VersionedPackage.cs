using System;
namespace PassivePicasso.ThunderKit.Thunderstore.Editor
{

    [Serializable]
    public partial class VersionedPackage
    {
        public string name;
        public string full_name;
        public string description;
        public Uri icon;
        public Uri website_url;
        public Version version_number;
        public string[] dependencies;
        public Uri download_url;
        public long downloads;
        public DateTimeOffset date_created;
        public bool is_active;
        public string uuid4;
    }
}

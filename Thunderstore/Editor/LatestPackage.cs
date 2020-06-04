using System;
namespace PassivePicasso.ThunderKit.Thunderstore.Editor
{

    [Serializable]
    public partial class LatestPackage
    {
        public string name;
        public string full_name;
        public string description;
        public Uri icon;
        public string version_number;
        public string[] dependencies;
        public Uri download_url;
        public long downloads;
        public DateTimeOffset date_created;
        public string website_url;
        public bool is_active;
    }
}

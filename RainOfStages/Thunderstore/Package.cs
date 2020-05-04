using System;
namespace RainOfStages.Thunderstore
{
    [Serializable]
    public partial class Package
    {
        public string name;
        public string full_name;
        public string owner;
        public Uri package_url;
        public DateTimeOffset date_created;
        public DateTimeOffset date_updated;
        public long rating_score;
        public bool is_pinned;
        public bool is_deprecated;
        public long total_downloads;
        public LatestPackage latest;
    }
}

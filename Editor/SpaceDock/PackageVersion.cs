using System;

namespace ThunderKit.Integrations.SpaceDock
{

    [Serializable]
    public partial class PackageVersion
    {
        public string friendly_version;
        public string game_version;
        public int id;
        public DateTime created;
        public string download_path;
        public string changelog;
        public int downloads;
    }
}
using System;
using System.Collections.Generic;

namespace ThunderKit.Integrations.SpaceDock
{
    [Serializable]
    public partial class PackageListing
    {
        public string name;
        public int id;
        public string game;
        public int game_id;
        public string short_description;
        public int downloads;
        public int followers;
        public string author;
        public int default_version_id;
        public List<string> shared_authors;
        public string background;
        public int bg_offset_y;
        public string license;
        public string website;
        public string donations;
        public string source_code;
        public string url;
        public List<PackageVersion> versions;
    }
}
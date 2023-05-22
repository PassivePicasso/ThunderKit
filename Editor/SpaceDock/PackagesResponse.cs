using System;

namespace ThunderKit.Integrations.SpaceDock
{
    [Serializable]
    public class PackagesResponse
    {
        public int count;
        public int pages;
        public int page;
        public PackageListing[] result;
    }
}
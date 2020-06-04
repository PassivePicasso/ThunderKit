using System;
namespace PassivePicasso.ThunderKit.Thunderstore.Editor
{
    [Serializable]
    public partial class Page
    {
        public long count;
        public Uri next;
        public object previous;
        public Package[] results;
    }
}

using System;
namespace RainOfStages.RainOfStagesShared.AutoConfig.Editor.Thunderstore
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

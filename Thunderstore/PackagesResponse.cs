#if UNITY_EDITOR
using System;

namespace ThunderKit.Thunderstore
{
    [Serializable]
    public class PackagesResponse 
    {
        public Package[] results;
    }
}
#endif
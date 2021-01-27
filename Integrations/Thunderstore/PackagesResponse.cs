#if UNITY_EDITOR
using System;

namespace ThunderKit.Integrations.Thunderstore
{
    [Serializable]
    public class PackagesResponse 
    {
        public Package[] results;
    }
}
#endif
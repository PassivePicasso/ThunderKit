﻿#if UNITY_EDITOR
using System;

namespace PassivePicasso.ThunderKit.Thunderstore
{
    [Serializable]
    public class PackagesResponse 
    {
        public Package[] results;
    }
}
#endif
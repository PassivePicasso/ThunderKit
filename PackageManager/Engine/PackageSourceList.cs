using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderKit.PackageManager.Engine
{
    public class PackageSourceList : ScriptableObject
    {
        public string SourceName;
        public List<PackageGroup> packages;
        public DateTime lastUpdateTime;
    }
}
using System;
using System.Collections.Generic;
using ThunderKit.PackageManager.Model;
using UnityEngine;

namespace ThunderKit.PackageManager.Editor
{
    public class PackageSourceList : ScriptableObject
    {
        public string SourceName;
        public List<PackageGroup> packages;
    }
}
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ThunderKit.PackageManager.Model
{
    [Serializable]
    public class PackageGroup
    {
        public string author;
        public string name;
        public string version;
        public string package_url;
        public Texture2D icon;
        public string description;
        public string dependencyId;
        public string[] dependencies;
        public PackageSource Source;
        public PackageVersion[] versions;
    }

    [Serializable]
    public class PackageVersion
    {
        public string version;
        public string dependencyId;
    }
}
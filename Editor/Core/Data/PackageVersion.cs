using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderKit.Core.Data
{
    [Serializable]
    public class PackageVersion 
    {
        public string version;
        public string dependencyId;
        public string VersionMarkdown;
        public PackageGroup group;
        public PackageVersion[] dependencies;
        internal string name;

        public override bool Equals(object obj)
        {
            return obj is PackageVersion version &&
                   dependencyId == version.dependencyId;
        }

        public override int GetHashCode()
        {
            return 996503521 + EqualityComparer<string>.Default.GetHashCode(dependencyId);
        }
    }
}
using System;

namespace ThunderKit.PackageManager.Engine
{
    [Serializable]
    public class PackageVersion
    {
        public string version;
        public string dependencyId;
    }
}
using System;
using System.Globalization;
using System.Linq;
using ThunderKit.PackageManager.Model;
using UnityEngine;

namespace ThunderKit.PackageManager.Engine
{
    [Serializable]
    public class PackageGroup
    {
        public PackageVersion this[string version] => versions.FirstOrDefault(pv=> pv.version.Equals(version));

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

        public bool HasString(string value)
        {
            var authorContains = CultureInfo.InvariantCulture.CompareInfo.IndexOf(author, value, CompareOptions.OrdinalIgnoreCase) > -1;
            if (authorContains) return true;
            var nameContains = CultureInfo.InvariantCulture.CompareInfo.IndexOf(name, value, CompareOptions.OrdinalIgnoreCase) > -1;
            if (nameContains) return true;
            var versionContains = CultureInfo.InvariantCulture.CompareInfo.IndexOf(version, value, CompareOptions.OrdinalIgnoreCase) > -1;
            if (versionContains) return true;
            var package_urlContains = CultureInfo.InvariantCulture.CompareInfo.IndexOf(package_url, value, CompareOptions.OrdinalIgnoreCase) > -1;
            if (package_urlContains) return true;
            var descriptionContains = CultureInfo.InvariantCulture.CompareInfo.IndexOf(description, value, CompareOptions.OrdinalIgnoreCase) > -1;
            if (descriptionContains) return true;
            var dependencyIdContains = CultureInfo.InvariantCulture.CompareInfo.IndexOf(dependencyId, value, CompareOptions.OrdinalIgnoreCase) > -1;
            if (dependencyIdContains) return true;

            return false;
        }
    }

}
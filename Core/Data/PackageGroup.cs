using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ThunderKit.Core.Data
{
    [Serializable]
    public class PackageGroup : IEquatable<PackageGroup>
    {
        public PackageVersion this[string version] => versions.FirstOrDefault(pv=> pv.version.Equals(version));

        public string author;
        public string name;
        public string version;
        public string package_url;
        public Texture2D icon;
        public string description;
        public string dependencyId;
        public string[] tags;
        public PackageSource Source;
        public PackageVersion[] versions;
        public string PackageDirectory => Path.Combine("Packages", name);
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
            foreach (var tag in tags)
            {
                var tagContains = CultureInfo.InvariantCulture.CompareInfo.IndexOf(tag, value, CompareOptions.OrdinalIgnoreCase) > -1;
                if (tagContains) return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PackageGroup);
        }

        public bool Equals(PackageGroup other)
        {
            return other != null &&
                   dependencyId == other.dependencyId;
        }

        public override int GetHashCode()
        {
            return 996503521 + EqualityComparer<string>.Default.GetHashCode(dependencyId);
        }

        public static bool operator ==(PackageGroup left, PackageGroup right)
        {
            return EqualityComparer<PackageGroup>.Default.Equals(left, right);
        }

        public static bool operator !=(PackageGroup left, PackageGroup right)
        {
            return !(left == right);
        }
    }

}
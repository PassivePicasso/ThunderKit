using System;
using ThunderKit.Core.Utilities;
using UnityEngine;

namespace ThunderKit.Core.Config
{
    /// <summary>
    /// Base ImportExtension for building GameImport extension points
    /// </summary>
    [Serializable]
    public abstract class ImportExtension<T> : IComparable<T> where T : ImportExtension<T>
    {
        public string Identity => PackageHelper.GetStringHashUTF8(GetType().AssemblyQualifiedName);

        public abstract string Name { get; }

        public bool Enabled
        {
            get
            {
                return PlayerPrefs.GetInt($"TKIE_{Identity}") == 1;
            }
            set
            {
                PlayerPrefs.SetInt($"TKIE_{Identity}", value ? 1 : 0);
            }
        }

        public virtual int CompareTo(T other)
        {
            return 0;
        }
    }
}
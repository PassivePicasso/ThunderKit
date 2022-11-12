using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using ThunderKit.Core.Data;
using UnityEditor;

namespace ThunderKit.Core.Utilities
{
    public static class Extensions
    {
        public static IEnumerable<IncludedSettings> GetFlags(this IncludedSettings input)
        {
            foreach (IncludedSettings value in (IncludedSettings[])Enum.GetValues(typeof(IncludedSettings)))
                if ((input & value) == value)
                    yield return value;
        }
        public static IEnumerable<FileAttributes> GetFlags(this FileAttributes input)
        {
            foreach (FileAttributes value in (FileAttributes[])Enum.GetValues(typeof(FileAttributes)))
                if ((input & value) == value)
                    yield return value;
        }

        public static bool HasFlag(this FileAttributes input, FileAttributes flag)
        {
            return (input & flag) == flag;
        }
        public static bool HasFlag(this ExportPackageOptions input, ExportPackageOptions flag)
        {
            return (input & flag) == flag;
        }

        public static bool TryGetValue<TValue>(this IDictionary<string, object> dict, string key, out TValue value)
        {
            if (!dict.TryGetValue(key, out var objValue))
            {
                value = default;
                return false;
            }

            value = (TValue)objValue;
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using ThunderKit.Common;
using ThunderKit.Core.Editor;
using UnityEngine;

namespace ThunderKit.Core.Data
{
    public class PackageSourceList : ScriptableObject
    {
        public string SourceName;
        public List<PackageGroup> packages;
        public DateTime lastUpdateTime;


        public static PackageSourceList GetPackageSourceList(PackageSource source) => ScriptableHelper.EnsureAsset<PackageSourceList>(
                                    $"{Constants.ThunderKitSettingsRoot}{source.Name}_SourceSettings.asset",
                                    psl => psl.SourceName = source.Name);
    }
}
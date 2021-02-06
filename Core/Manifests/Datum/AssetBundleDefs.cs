using System;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Manifests.Datums
{

    [Serializable]
    public struct AssetBundleDef
    {
        public string assetBundleName;
        public UnityEngine.Object[] assets;
    }

    public class AssetBundleDefs : ManifestDatum
    {
        [Tooltip("Forbidden Assets: Add Folders and Assets here to forbid them from being includeded in these AssetBundles.  Folders will be recursed to find all assets contained in them.")]
        public DefaultAsset[] ForbiddenAssets;
        public AssetBundleDef[] assetBundles;
    }
}
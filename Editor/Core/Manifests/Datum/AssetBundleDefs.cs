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
        public AssetBundleDef[] assetBundles;
    }
}
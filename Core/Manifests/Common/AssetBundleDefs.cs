using System;

namespace PassivePicasso.ThunderKit.Core.Manifests.Common
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
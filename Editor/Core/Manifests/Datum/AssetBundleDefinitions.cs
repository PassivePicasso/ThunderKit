﻿using System;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Manifests.Datum
{

    [Serializable]
    public struct AssetBundleDefinition
    {
        public string assetBundleName;
        public UnityEngine.Object[] assets;
    }

    public class AssetBundleDefinitions : ManifestDatum
    {
        public AssetBundleDefinition[] assetBundles;
    }
}
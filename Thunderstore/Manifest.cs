#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore
{
    public class Manifest : ScriptableObject
    {
        public string author;
        public string version_number;
        public string website_url;
        public string description;
        public List<string> dependencies;
        public UnityPackage[] unityPackages;

        public AssemblyDefinitionAsset[] plugins;
        public AssemblyDefinitionAsset[] patchers;
        public AssemblyDefinitionAsset[] monomod;
        public AssetBundleDef[] assetBundles;

        public TextAsset readme;
        public Texture2D icon;

        private string GetNamePrefix() => $"{{\"name\":\"{name}\",";
        public string RenderJson()
        {
            var manifestJson = JsonUtility.ToJson(this);

            manifestJson = manifestJson.Substring(1);
            manifestJson = $"{GetNamePrefix()}{manifestJson}";
            return manifestJson;
        }

        [Serializable]
        public struct AssetBundleDef
        {
            public string assetBundleName;
            public UnityEngine.Object[] assets;
        }

    }
}
#endif
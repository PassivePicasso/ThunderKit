using PassivePicasso.ThunderKit.Utilities;
using System.Collections.Generic;
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
        public UnityPackage[] redistributables;

        public AssemblyDefinitionAsset[] plugins;
        public AssemblyDefinitionAsset[] patchers;
        public AssemblyDefinitionAsset[] monomod;
        public AssetBundleManifest assetBundleManifest;

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


    }
}
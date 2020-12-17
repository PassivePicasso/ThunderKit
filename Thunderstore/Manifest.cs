using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore.Editor
{
    public class Manifest : ScriptableObject
    {
        //public new string name;
        public string author;
        public string version_number;
        public string website_url;
        public string description;
        public List<string> dependencies;

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
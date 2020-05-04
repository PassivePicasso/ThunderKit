using UnityEngine;
#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;
#endif

namespace RainOfStages.Thunderstore
{
    public class Manifest : ScriptableObject
    {
        public string version_number;
        public string website_url;
        public string description;
        public string[] dependencies;

#if UNITY_EDITOR
        [MenuItem("Assets/Rain of Stages/Mod Manifest")]
        public static void Create()
        {
            ScriptableHelper.CreateAsset<Manifest>();
        }
#endif
    }
}
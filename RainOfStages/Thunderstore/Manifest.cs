using RainOfStages.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RainOfStages.Thunderstore
{
    public class Manifest : ScriptableObject
    {
        public string version_number;
        public string website_url;
        public string description;
        public string[] dependencies;

        [MenuItem("Assets/Rain of Stages/Mod Manifest")]
        public static void Create()
        {
            ScriptableHelper.CreateAsset<Manifest>("Manifest");
        }
    }
}
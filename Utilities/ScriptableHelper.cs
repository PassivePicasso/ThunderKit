#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Utilities
{
    public static class ScriptableHelper
    {
        public const string ThunderKitContextRoot = "Assets/ThunderKit/";
        public const string ThunderKitMenuRoot = "ThunderKit/";

        public static void CreateAsset<T>(Func<string> overrideName = null) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            var name = overrideName == null ? typeof(T).Name : overrideName();
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/{name}.asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        public static T EnsureAsset<T>(string assetPath, Action<T> initializer) where T : ScriptableObject
        {
            var settings = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<T>();
                initializer(settings);
                AssetDatabase.CreateAsset(settings, assetPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
    }
}
#endif
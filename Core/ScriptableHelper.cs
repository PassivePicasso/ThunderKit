#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Core
{
    public static class ScriptableHelper
    {
        public const string ThunderKitContextRoot = "Assets/ThunderKit/";
        public const string ThunderKitMenuRoot = "ThunderKit/";

        static object[] findTextureParams = new object[1];
        public static void SelectNewAsset<T>(Func<string> overrideName = null) where T : ScriptableObject
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
            var name = typeof(T).Name;
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/{name}.asset");

            void onCreated(int instanceId, string pathname, string resourceFile)
            {
                AssetDatabase.CreateAsset(asset, pathname);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = asset;
            }

            if (overrideName == null)
            {
                var endAction = ScriptableObject.CreateInstance<SelfDestructingActionAsset>();
                endAction.action = onCreated;
                var findTexture = typeof(EditorGUIUtility).GetMethod(nameof(EditorGUIUtility.FindTexture), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                findTextureParams[0] = typeof(T);
                var icon = (Texture2D)findTexture.Invoke(null, findTextureParams);
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(asset.GetInstanceID(), endAction, assetPathAndName, icon, null);
            }
            else
            {
                name = overrideName();
                assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/{name}.asset");

                onCreated(asset.GetInstanceID(), assetPathAndName, null);
            }
        }

        public static void SelectNewAsset(Type t, Func<string> overrideName = null)
        {
            if (!typeof(ScriptableObject).IsAssignableFrom(t)) return;

            var asset = ScriptableObject.CreateInstance(t);

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            var name = overrideName == null ? t.Name : overrideName();
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
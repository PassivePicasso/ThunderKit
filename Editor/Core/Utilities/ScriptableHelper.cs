using System;
using System.IO;
using ThunderKit.Core.Actions;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Utilities
{
    public static class ScriptableHelper
    {
        readonly static object[] findTextureParams = new object[1];
        /// <summary>
        /// Creates and Saves a ScriptableObject of Type T allowing the user the input the name of the new asset, or cancel by pressing escape
        /// The asset will be created in one of the folowing:
        ///     The selected folder
        ///     The containing folder of a selected asset
        ///     The Assets folder if there is no selection in the Project window
        /// </summary>
        /// <typeparam name="T">Type of ScriptableObject to create</typeparam>
        /// <param name="afterCreated">Delegate to run after asset creation process has completed</param>
        public static void SelectNewAsset<T>(Action<T> afterCreated = null) where T : ScriptableObject
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
            Action<int, string, string> action =
                (int instanceId, string pathname, string resourceFile) =>
                  {
                      AssetDatabase.CreateAsset(asset, pathname);
                      AssetDatabase.SaveAssets();
                      AssetDatabase.Refresh();
                      Selection.activeObject = asset;
                      afterCreated?.Invoke(asset);
                  };

            var endAction = ScriptableObject.CreateInstance<SelfDestructingActionAsset>();
            endAction.action = action;
            var findTexture = typeof(EditorGUIUtility).GetMethod(nameof(EditorGUIUtility.FindTexture), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            findTextureParams[0] = typeof(T);
            var icon = (Texture2D)findTexture.Invoke(null, findTextureParams);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(asset.GetInstanceID(), endAction, assetPathAndName, icon, null);
        }

        /// <summary>
        /// Assign a Texture2d as the icon for a UnityEngine.Object in the Unity Editor
        /// </summary>
        /// <param name="g">Object to assign icon to</param>
        /// <param name="tex">icon to assign to Object</param>
        public static void AssignIcon(UnityEngine.Object g, Texture2D tex)
        {
            Type editorGUIUtilityType = typeof(EditorGUIUtility);
            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
            object[] args = new object[] { g, tex };
            editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
        }

        /// <summary>
        /// Creates and Saves a ScriptableObject of Type t
        /// The asset will be created in one of the folowing:
        ///     The selected folder
        ///     The containing folder of a selected asset
        ///     The Assets folder if there is no selection in the Project window
        /// </summary>
        /// <param name="t">Type of ScriptableObject to create</param>
        /// <param name="overrideName">Delegate which returns a string to be assigned as the name of the new asset</param>
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

        /// <summary>
        /// if an Asset of Type T does not exist at assetPath, creates and saves a new asset of Type T
        /// </summary>
        /// <typeparam name="T">Type of ScriptableObject to create</typeparam>
        /// <param name="assetPath">Path to ScriptableObject</param>
        /// <param name="initializer">Delegate to run after asset creation process has completed</param>
        /// <returns>Created ScriptableObject</returns>
        public static T EnsureAsset<T>(string assetPath, Action<T> initializer = null) where T : ScriptableObject
        {
            var settings = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(settings, assetPath);
                initializer?.Invoke(settings);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        /// <summary>
        /// if an Asset of Type t does not exist at assetPath, creates and saves a new asset of Type t
        /// </summary>
        /// <param name="assetPath">Path to ScriptableObject</param>
        /// <param name="t">Type of ScriptableObject to create</typeparam>
        /// <param name="initializer">Delegate to run after asset creation process has completed</param>
        /// <returns>Created ScriptableObject</returns>
        public static object EnsureAsset(string assetPath, Type t, Action<object> initializer = null)
        {
            if (!typeof(ScriptableObject).IsAssignableFrom(t)) throw new ArgumentException("Paramater Type type must be of type ScriptableObject");

            var settings = AssetDatabase.LoadAssetAtPath(assetPath, t);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance(t);
                AssetDatabase.CreateAsset(settings, assetPath);
                initializer?.Invoke(settings);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings), ImportAssetOptions.ForceUpdate);
            }
            return settings;
        }
    }
}
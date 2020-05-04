using System.IO;
using UnityEditor;
using UnityEngine;

namespace RainOfStages.Editor
{
    public static class ScriptableHelper
    {
        //public static void CreateAsset<T>(string name) where T : ScriptableObject
        //{
        //    var scriptable = ScriptableObject.CreateInstance<T>();
        //    var path = GetClickedDirFullPath();
        //    scriptable.name = name;

        //    AssetDatabase.CreateAsset(scriptable, Path.Combine(path, $"{name}.asset").Replace('\\', '/'));
        //    AssetDatabase.Refresh();
        //}

        //private static string GetClickedDirFullPath()
        //{
        //    string clickedAssetGuid = Selection.assetGUIDs[0];
        //    string clickedPath = AssetDatabase.GUIDToAssetPath(clickedAssetGuid);
        //    string clickedPathFull = Path.Combine(Directory.GetCurrentDirectory(), clickedPath);

        //    FileAttributes attr = File.GetAttributes(clickedPathFull);
        //    var path = attr.HasFlag(FileAttributes.Directory) ? clickedPathFull : Path.GetDirectoryName(clickedPathFull);

        //    path = path.Replace('\\', '/');

        //    return path;
        //}
        public static void CreateAsset<T>(string name) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
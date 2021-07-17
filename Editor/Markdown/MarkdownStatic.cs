using System;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace ThunderKit.Markdown
{
    public static class MarkdownStatic
    {
        readonly static object[] findTextureParams = new object[1];
        class SelfDestructingActionAsset : EndNameEditAction
        {
            public Action<int, string, string> action;

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                action(instanceId, pathName, resourceFile);
                CleanUp();
            }
        }

        [MenuItem("Assets/ThunderKit/Markdown File")]
        public static void Create()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/MarkdownFile.md");

            AssetDatabase.Refresh();

            Action<int, string, string> action =
                (int instanceId, string pathname, string resourceFile) =>
                {
                    File.WriteAllText(pathname, "# Markdown File");
                    AssetDatabase.Refresh();
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(pathname);
                };

            var endAction = ScriptableObject.CreateInstance<SelfDestructingActionAsset>();
            endAction.action = action;
            var findTexture = typeof(EditorGUIUtility).GetMethod(nameof(EditorGUIUtility.FindTexture), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            findTextureParams[0] = typeof(TextAsset);
            var icon = (Texture2D)findTexture.Invoke(null, findTextureParams);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, endAction, assetPathAndName, icon, null);
        }
    }
}
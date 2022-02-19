using System;
using System.IO;
using ThunderKit.Common;
using ThunderKit.Core.Actions;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Documentation
{
    public static class DocumentationHelpers
    {
        readonly static object[] findTextureParams = new object[1];

        [MenuItem(Constants.ThunderKitContextRoot + "Documentation Folder", priority = Constants.ThunderKitMenuPriority)]
        public static void CreateFolder()
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
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"NewDocumentationFolder.md");

            AssetDatabase.Refresh();

            Action<int, string, string> action =
                (int instanceId, string markPath, string resourceFile) =>
                {
                    var name = Path.GetFileNameWithoutExtension(markPath);
                    var rootPath = Path.GetDirectoryName(markPath);

                    var directory = Path.Combine(rootPath, name);
                    var title = ObjectNames.NicifyVariableName(name);

                    Directory.CreateDirectory(directory);
                    File.WriteAllText(markPath, $"# {title}");
                    AssetDatabase.Refresh();
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(markPath);
                };

            var endAction = ScriptableObject.CreateInstance<SelfDestructingActionAsset>();
            endAction.action = action;
            var findTexture = typeof(EditorGUIUtility).GetMethod(nameof(EditorGUIUtility.FindTexture), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            findTextureParams[0] = typeof(DefaultAsset);
            var icon = (Texture2D)findTexture.Invoke(null, findTextureParams);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, endAction, assetPathAndName, icon, null);
        }
    }
}

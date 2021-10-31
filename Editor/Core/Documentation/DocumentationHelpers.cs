using System;
using System.Collections;
using System.Collections.Generic;
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
                    var uxmlPath = Path.Combine(rootPath, $"{name}.uxml");
                    var title = ObjectNames.NicifyVariableName(name);

                    Directory.CreateDirectory(directory);
                    File.WriteAllText(markPath, $"# {title}");
                    File.WriteAllText(uxmlPath, GetUXMLTemplate(title));
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

        [MenuItem(Constants.ThunderKitContextRoot + "Documentation Page", priority = Constants.ThunderKitMenuPriority)]
        public static void CreatePage()
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
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"NewDocumentationPage");

            AssetDatabase.Refresh();

            Action<int, string, string> action =
                (int instanceId, string pathname, string resourceFile) =>
                {
                    var ext = Path.GetExtension(pathname);
                    if (string.IsNullOrEmpty(ext))
                        pathname = $"{pathname}.md";

                    var title = ObjectNames.NicifyVariableName(Path.GetFileNameWithoutExtension(pathname));
                    var uxmlPathname = Path.ChangeExtension(pathname, ".uxml");
                    File.WriteAllText(pathname, $"# {title}");
                    File.WriteAllText(uxmlPathname, GetUXMLTemplate(title));
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

        static string GetUXMLTemplate(string title) =>
$@"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" editor-extension-mode=""True"">
    <Style src=""/Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss"" />
    <ui:VisualElement name=""help-page"" class=""m4"">
        <ui:VisualElement name=""header"" class=""bm4"" style=""flex-direction: row; flex-basis: 64px; justify-content: flex-start; align-items: center; background-color: rgba(0, 0, 0, 0.39);"">
            <ui:VisualElement name=""icon"" class=""header-icon TK_Documentation_2X_Icon"" />
            <ui:Label text=""{title}"" display-tooltip-when-elided=""true"" name=""title"" class=""page-header"" style=""justify-content: center; margin-left: 10px;"" />
        </ui:VisualElement>
        <ThunderKit.Markdown.MarkdownElement />
    </ui:VisualElement>
</ui:UXML>";

    }
}

using System.Diagnostics;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderKit.Core.Editor
{
    public static class ScriptEditorHelper
    {
        public static void EditScript(ScriptableObject scriptableObject)
        {
            var script = MonoScript.FromScriptableObject(scriptableObject);
            var scriptPath = AssetDatabase.GetAssetPath(script);
            var editorType = ScriptEditorUtility.GetScriptEditorFromPreferences();
            var editorPath = ScriptEditorUtility.GetExternalScriptEditor();
            var args = ScriptEditorUtility.GetExternalScriptEditorArgs();
            switch (editorType)
            {
                case ScriptEditorUtility.ScriptEditor.SystemDefault:
                case ScriptEditorUtility.ScriptEditor.MonoDevelop:
                case ScriptEditorUtility.ScriptEditor.VisualStudioExpress:
                case ScriptEditorUtility.ScriptEditor.Rider:
                    UnityEngine.Debug.LogError($"Code Editor: {editorType} not supported by ComposableObject Edit Script command");
                    break;
                case ScriptEditorUtility.ScriptEditor.VisualStudio:
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = editorPath,
                        Arguments = $"{args} /Edit {scriptPath}"
                    });
                    break;
                case ScriptEditorUtility.ScriptEditor.Other:
                case ScriptEditorUtility.ScriptEditor.VisualStudioCode:
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = editorPath,
                        Arguments = $"{args} {scriptPath}"
                    });
                    break;
            }

        }
    }
}
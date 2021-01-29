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

            InternalEditorUtility.OpenFileAtLineExternal(scriptPath, -1);

        }
    }
}
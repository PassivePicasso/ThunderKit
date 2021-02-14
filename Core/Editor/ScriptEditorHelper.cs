using System.IO;
using ThunderKit.Core.Editor.Controls;
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
        public static (string destinationPath, string nameSpace, string fileName) GetDetails(string typeFullName) => (
                          destinationPath: Path.Combine("Assets", $"{typeFullName}.cs").Replace("\\", "/"),
                          nameSpace: Path.GetDirectoryName(typeFullName).Replace('/', '.').Replace('\\', '.'),
                          fileName: Path.GetFileNameWithoutExtension(typeFullName)
                      );

        /// <summary>
        /// Generates a new script at the specified path.
        /// Type will generate with a namespace equal to the given path
        /// </summary>
        /// <param name="template"></param>
        /// <param name="newScriptPath">Path relative to the project's Assets folder</param>
        public static void GenerateAndLoadScript(string template, string newScriptPath)
        {
            var (destinationPath, ns, fileName) = GetDetails(newScriptPath);

            if (string.IsNullOrEmpty(template)) return;

            var backup = NewScriptInfo.Instance;
            backup.addAsset = true;
            backup.scriptPath = destinationPath;
            EditorUtility.SetDirty(backup);

            var parentDirectoryPath = Path.GetDirectoryName(destinationPath);
            Directory.CreateDirectory(parentDirectoryPath);

            ns = string.IsNullOrEmpty(ns) ? "Assets" : ns;
            var rendered = string.Format(template, ns, fileName);
            File.WriteAllText(destinationPath, rendered);

            AssetDatabase.ImportAsset(destinationPath);
            AssetDatabase.Refresh();

        }

    }
}
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Editor.Actions
{
    public class DeletePackage : ScriptableObject
    {
        public string directory;

        public bool TryDelete()
        {
            try
            {
                if (EditorApplication.isCompiling) return false;
                if (Directory.Exists(directory))
                {
                    foreach (var metaFile in Directory.GetFiles(directory, "*.meta", SearchOption.AllDirectories))
                        File.Delete(metaFile);
                    Directory.Delete(directory, true);
                }
            }
            catch { }
            return !Directory.Exists(directory);
        }
    }
}
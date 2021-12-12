using System;
using System.IO;
using UnityEditor;

namespace ThunderKit.Markdown
{
    public class MarkdownFileWatcher : AssetPostprocessor
    {
        public enum ChangeType { Imported, Deleted, Moved }

        public static event EventHandler<(string path, ChangeType change)> DocumentUpdated;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string document in importedAssets)
            {
                if (Path.GetExtension(document) != ".md") continue;
                DocumentUpdated?.Invoke(null, (document, ChangeType.Imported));
            }
            foreach (string document in deletedAssets)
            {
                if (Path.GetExtension(document) != ".md") continue;
                DocumentUpdated?.Invoke(null, (document, ChangeType.Deleted));
            }
        }
    }
}
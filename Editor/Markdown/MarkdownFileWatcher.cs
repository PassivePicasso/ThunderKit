using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Markdown
{
    public class MarkdownFileWatcher : AssetPostprocessor
    {
        static Dictionary<string, HashSet<MarkdownElement>> ActiveElementDocuments = new Dictionary<string, HashSet<MarkdownElement>>();
        public static void RegisterActiveElement(string document, MarkdownElement element)
        {
            if (!ActiveElementDocuments.ContainsKey(document))
                ActiveElementDocuments[document] = new HashSet<MarkdownElement>();

            ActiveElementDocuments[document].Add(element);
        }
        public static void UnregisterActiveElement(string document, MarkdownElement element)
        {
            if (!ActiveElementDocuments.ContainsKey(document)) return;

            ActiveElementDocuments[document].Remove(element);
        }
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string document in importedAssets)
            {
                if (Path.GetExtension(document) != ".md") continue;
                if (!ActiveElementDocuments.ContainsKey(document)) continue;
                foreach (var element in ActiveElementDocuments[document])
                    element.RefreshContent();
            }
        }
    }
}
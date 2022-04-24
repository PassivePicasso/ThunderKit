using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.Core.Windows;
using ThunderKit.Markdown;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif


namespace ThunderKit.Core.Documentation
{
    public class MarkdownContextualMenuManipulator : ContextualMenuManipulator
    {
        public MarkdownContextualMenuManipulator() : base(OnContextMenu)
        {
        }

        static void OnContextMenu(ContextualMenuPopulateEvent evt)
        {
            var visualElement = evt.currentTarget as VisualElement;
            var picked = new List<VisualElement>();
            visualElement.panel.PickAll(evt.mousePosition, picked);
            var markdownElement = picked.OfType<MarkdownElement>().FirstOrDefault();
            if (markdownElement == null) return;
            if (markdownElement.MarkdownDataType == MarkdownDataType.Implicit || markdownElement.MarkdownDataType == MarkdownDataType.Source)
            {
                Action<DropdownMenu.MenuAction> editAction = dma => EditorSource(markdownElement);
                Action<DropdownMenu.MenuAction> selectAction = dma => SelectSource(markdownElement);
#if UNITY_2019_1_OR_NEWER
                evt.menu.AppendAction("Edit Source", editAction);
                evt.menu.AppendAction("Select Source", selectAction);
#else
                Func<DropdownMenu.MenuAction, DropdownMenu.MenuAction.StatusFlags> normalStatus = status => DropdownMenu.MenuAction.StatusFlags.Normal;
                evt.menu.AppendAction("Edit Source", editAction, normalStatus);
                evt.menu.AppendAction("Select Source", selectAction, normalStatus);
#endif
            }
        }

        private static void SelectSource(MarkdownElement markdownElement)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(markdownElement.Data);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
        }

        private static void EditorSource(MarkdownElement markdownElement)
        {
            try
            {
                string path = Path.GetFullPath(markdownElement.Data);
                if (File.Exists(path))
                {
                    if (string.Compare(Path.GetExtension(path), ".md", true) != 0)
                    {
                        throw new InvalidOperationException($"The file {path} is not a file with a markdown extension (.md)");
                    }
                    var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
                    switch (settings.MarkdownOpenMode)
                    {
                        case MarkdownOpenMode.OperatingSystemDefault:
                            System.Diagnostics.Process.Start(path);
                            break;
                        case MarkdownOpenMode.UnityExternalEditor:
                            InternalEditorUtility.OpenFileAtLineExternal(path, -1);
                            break;
                    }
                    return;
                }
                throw new NullReferenceException($"No file exists in {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Windows;
using ThunderKit.Core.Manifests.Datum;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;
using Debug = UnityEngine.Debug;
using ThunderKit.Common;
using UnityEngine.Profiling;
using ThunderKit.Core.Utilities;

namespace ThunderKit.Core.Inspectors
{
    [CustomEditor(typeof(ComposableObject), true)]
    public class ComposableObjectEditor : UnityEditor.Editor
    {
        const string MissingScriptReference = "Missing Script Reference";
        protected static GUISkin EditorSkin;

        public class StepData
        {
            public SerializedProperty step;
            public SerializedProperty dataArray;
            public int index;
        }

        static ComposableElement ClipboardItem;
        Dictionary<UnityEngine.Object, UnityEditor.Editor> Editors;
        SerializedProperty dataArray;

        protected virtual IEnumerable<string> ExcludedProperties()
        {
            yield break;
        }
        protected virtual Rect OnBeforeElementHeaderGUI(Rect rect, ComposableElement element, ref string title) => rect;
        protected virtual Rect OnAfterElementHeaderGUI(Rect rect, ComposableElement element) => rect;
        private void OnEnable()
        {
            try
            {
                var targetObject = target as ComposableObject;
                Editors = new Dictionary<UnityEngine.Object, UnityEditor.Editor>();
            }
            catch
            {
            }
        }
        public override void OnInspectorGUI()
        {
            Profiler.BeginSample("ComposableObjectEditor");
            //Evaluate Editor Skin settings
            if (!EditorSkin)
                if (EditorGUIUtility.isProSkin)
                    EditorSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(Constants.ThunderKitRoot + "/Skins/DarkSkin.guiskin");
                else
                    EditorSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(Constants.ThunderKitRoot + "/Skins/LightSkin.guiskin");

            //Ensure SerializedObject is up to date with latest data
            serializedObject.Update();
            var excludedProperties = ExcludedProperties().Append("m_Script").Append("Data").ToArray();
            EditorHelpers.DrawSanitizedPropertiesExcluding(serializedObject, excludedProperties);
            GUILayout.Space(4);

            dataArray = serializedObject.FindProperty(nameof(ComposableObject.Data));
            CleanDataArray();

            var boxSkin = EditorSkin.box;
            var popupIcon = IconContent("_Popup");
            var stopIcon = IconContent("console.erroricon.sml");

            for (int i = 0; i < dataArray.arraySize; i++)
            {
                var foldoutRect = GUILayoutUtility.GetRect(currentViewWidth - 50, singleLineHeight + 3);
                var standardSize = singleLineHeight + standardVerticalSpacing;
                var menuRect = new Rect(foldoutRect.x + 1 + foldoutRect.width - standardSize, foldoutRect.y + 1, standardSize, standardSize);

                GUI.Box(new Rect(foldoutRect.x - 24, foldoutRect.y - 1, foldoutRect.width + 30, foldoutRect.height + 1), new GUIContent(string.Empty), boxSkin);

                var step = dataArray.GetArrayElementAtIndex(i);
                var element = step.objectReferenceValue as ComposableElement;
                var title = string.Empty;
                Editor editor = null;
                if (element)
                {
                    title = $"{i} - {ObjectNames.NicifyVariableName(element.GetType().Name)}";
                    if (!Editors.TryGetValue(element, out editor))
                        Editors[element] = editor = CreateEditor(element);
                }
                else
                    title = $"{i} - {MissingScriptReference}";


                try
                {
                    switch (Event.current.type)
                    {
                        case EventType.Repaint:
                            GUIStyle.none.Draw(menuRect, popupIcon, false, false, false, false);
                            break;
                        case EventType.MouseUp when menuRect.Contains(Event.current.mousePosition):
                            ShowContextMenu(i, step);
                            break;
                    }
                }
                catch (Exception e) { Debug.LogException(e); }

                try
                {
                    foldoutRect = OnBeforeElementHeaderGUI(foldoutRect, element, ref title);
                }
                catch (Exception e) { Debug.LogException(e); }

                if (!editor)
                {
                    foldoutRect.x -= stopIcon.image.width;
                    EditorGUI.LabelField(foldoutRect, new GUIContent(title, stopIcon.image));
                }
                else
                {
                    step.isExpanded = EditorGUI.Foldout(foldoutRect, step.isExpanded, title);
                    if (step.isExpanded)
                        try
                        {
                            editor.serializedObject.UpdateIfRequiredOrScript();
                            EditorGUI.BeginChangeCheck();
                            editor.OnInspectorGUI();
                            if (EditorGUI.EndChangeCheck())
                            {
                                editor.serializedObject.ApplyModifiedProperties();
                                Repaint();
                            }
                        }
                        catch (Exception e) { Debug.LogException(e); }
                }
                try
                {
                    foldoutRect = OnAfterElementHeaderGUI(foldoutRect, element);
                }
                catch (Exception e) { Debug.LogException(e); }

                try
                {
                    foldoutRect.width -= menuRect.width;
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && foldoutRect.Contains(Event.current.mousePosition))
                        step.isExpanded = !step.isExpanded;
                }
                catch (Exception e) { Debug.LogException(e); }

            }

            var composableObject = target as ComposableObject;
            var size = new Vector2(250, 24);
            var rect = GUILayoutUtility.GetRect(size.x, size.y);
            rect.width = size.x;
            rect.y += standardVerticalSpacing;
            rect.x = (currentViewWidth / 2) - (rect.width / 2);
            OnAddElementGUI(rect, composableObject);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(serializedObject.targetObject);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }
            Profiler.EndSample();
        }
        private void ShowContextMenu(int i, SerializedProperty step)
        {
            var menu = new GenericMenu();
            var stepData = new StepData { step = step, index = i, dataArray = dataArray };
            Action<string, bool, GenericMenu.MenuFunction2> AddMenuItemAction = (header, toggled, action) =>
            {
                if (step.objectReferenceValue)
                    menu.AddItem(new GUIContent(header), toggled, action, stepData);
                else
                    menu.AddDisabledItem(new GUIContent(header));
            };

            if (step.objectReferenceValue is ManifestIdentity)
                menu.AddDisabledItem(new GUIContent(ObjectNames.NicifyVariableName(nameof(Remove))));
            else
                AddMenuItemAction((ObjectNames.NicifyVariableName(nameof(Remove))), false, Remove);

            menu.AddSeparator("");
            AddMenuItemAction((ObjectNames.NicifyVariableName(nameof(Duplicate))), false, Duplicate);
            AddMenuItemAction((ObjectNames.NicifyVariableName(nameof(Copy))), false, Copy);

            var currentroot = step.serializedObject.targetObject as ComposableObject;

            if (ClipboardItem && currentroot.ElementType.IsAssignableFrom(ClipboardItem.GetType()))
            {
                menu.AddItem(new GUIContent($"Paste {ObjectNames.NicifyVariableName(ClipboardItem.name)} above"), false, PasteNewAbove, stepData);
                menu.AddItem(new GUIContent($"Paste {ObjectNames.NicifyVariableName(ClipboardItem.name)} below"), false, PasteNew, stepData);

                AddMenuItemAction($"Paste {ObjectNames.NicifyVariableName(ClipboardItem.name)} values", false, PasteValues);
            }
            else
                menu.AddDisabledItem(new GUIContent($"Paste"));

            menu.AddSeparator("");
            AddMenuItemAction((ObjectNames.NicifyVariableName(nameof(MoveToTop))), false, MoveToTop);
            AddMenuItemAction((ObjectNames.NicifyVariableName(nameof(MoveUp))), false, MoveUp);
            AddMenuItemAction((ObjectNames.NicifyVariableName(nameof(MoveDown))), false, MoveDown);
            AddMenuItemAction((ObjectNames.NicifyVariableName(nameof(MoveToBottom))), false, MoveToBottom);
            menu.AddSeparator("");
            AddMenuItemAction((ObjectNames.NicifyVariableName(nameof(EditScript))), false, EditScript);
            menu.ShowAsContext();
        }

        private void CleanDataArray()
        {
            for (int i = 0; i < dataArray.arraySize; i++)
            {
                SerializedProperty step;
                do
                {
                    step = dataArray.GetArrayElementAtIndex(i);
                    if (!step.objectReferenceValue)
                    {
                        for (int x = i; x < dataArray.arraySize - 1; x++)
                            dataArray.MoveArrayElement(x + 1, x);
                    }
                }
                while (step == null);
            }
        }
        static void EditScript(object data)
        {
            if (data is StepData stepData
             && stepData.step.objectReferenceValue is ScriptableObject scriptableObject)
                ScriptEditorHelper.EditScript(scriptableObject);
        }
        static void Duplicate(object data)
        {
            var stepData = data as StepData;
            if (stepData.index == 0) return;

            var instance = (ComposableElement)Instantiate(stepData.step.objectReferenceValue);

            var target = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(stepData.step.serializedObject.targetObject)) as ComposableObject;
            target.InsertElement(instance, stepData.index);
        }
        static void PasteNewAbove(object data)
        {
            var stepData = data as StepData;

            if (ClipboardItem)
                InsertClipboard(stepData, 0);
        }
        static void PasteNew(object data)
        {
            var stepData = data as StepData;

            if (ClipboardItem)
                InsertClipboard(stepData, 1);
        }
        static void PasteValues(object data)
        {
            var stepData = data as StepData;
            var targetProperty = stepData.dataArray.GetArrayElementAtIndex(stepData.index);
            var target = targetProperty.objectReferenceValue;
            if (ClipboardItem && ClipboardItem.GetType().IsAssignableFrom(target.GetType()))
            {
                Undo.RecordObject(target, "Overwrite values");
                var clipboardJson = JsonUtility.ToJson(ClipboardItem);
                JsonUtility.FromJsonOverwrite(clipboardJson, target);
            }
        }
        private static void InsertClipboard(StepData stepData, int offset)
        {
            var target = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(stepData.step.serializedObject.targetObject)) as ComposableObject;
            target.InsertElement(ClipboardItem, stepData.index + offset);
            ClipboardItem = null;
        }
        static void Copy(object data)
        {
            var stepData = data as StepData;
            if (ClipboardItem) DestroyImmediate(ClipboardItem);
            ClipboardItem = (ComposableElement)Instantiate(stepData.step.objectReferenceValue);
            ClipboardItem.name = ClipboardItem.name.Replace("(Clone)", "");
        }
        static void MoveToTop(object data)
        {
            var stepData = data as StepData;
            if (stepData.index == 0) return;
            stepData.dataArray.MoveArrayElement(stepData.index, 0);
            stepData.dataArray.serializedObject.SetIsDifferentCacheDirty();
            stepData.dataArray.serializedObject.ApplyModifiedProperties();
        }
        static void MoveToBottom(object data)
        {
            var stepData = data as StepData;
            if (stepData.index == stepData.dataArray.arraySize - 1) return;
            stepData.dataArray.MoveArrayElement(stepData.index, stepData.dataArray.arraySize - 1);
            stepData.dataArray.serializedObject.SetIsDifferentCacheDirty();
            stepData.dataArray.serializedObject.ApplyModifiedProperties();
        }
        static void MoveUp(object data)
        {
            var stepData = data as StepData;
            if (stepData.index == 0) return;
            stepData.dataArray.MoveArrayElement(stepData.index, stepData.index - 1);
            stepData.dataArray.serializedObject.SetIsDifferentCacheDirty();
            stepData.dataArray.serializedObject.ApplyModifiedProperties();
        }
        static void MoveDown(object data)
        {
            var stepData = data as StepData;
            if (stepData.index == stepData.dataArray.arraySize - 1) return;
            stepData.dataArray.MoveArrayElement(stepData.index, stepData.index + 1);
            stepData.dataArray.serializedObject.SetIsDifferentCacheDirty();
            stepData.dataArray.serializedObject.ApplyModifiedProperties();
        }
        static void Remove(object data)
        {
            var stepData = data as StepData;
            var target = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(stepData.step.serializedObject.targetObject)) as ComposableObject;
            var composableElement = stepData.step.objectReferenceValue as ComposableElement;
            target.RemoveElement(composableElement, stepData.index);
        }

        AddComposableElementWindow popup;
        void OnAddElementGUI(Rect rect, ComposableObject composableObject)
        {
            var createFromScript = new Func<MonoScript, ScriptableObject>((script) =>
            {
                if (!script) return null;
                if (script.GetClass() == null) return null;

                var instance = (ComposableElement)CreateInstance(script.GetClass());
                instance.name = script.GetClass().Name;
                composableObject.InsertElement(instance, dataArray.arraySize);
                return instance;
            });

            if (AddComposableElementWindow.HasAssetToAdd())
                AddComposableElementWindow.Backup(createFromScript);

            if (GUI.Button(rect, $"Add {ObjectNames.NicifyVariableName(composableObject.ElementType.Name)}"))
            {
                if (popup && HasFlag(Event.current.modifiers, EventModifiers.Control))
                {
                    popup.StaysOpen = false;
                    popup.Focus();
                    popup = null;
                    return;
                }

                var fudge = EditorGUIUtility.currentViewWidth % 2 == 0 ? 0 : 1;
                var windowRect = new Rect(rect.x - fudge, rect.y + rect.height, rect.width, 200);
                var minXY = GUIUtility.GUIToScreenPoint(windowRect.min);
                windowRect = new Rect(minXY.x, minXY.y, windowRect.width, windowRect.height);
                popup = CreateInstance<AddComposableElementWindow>();
                popup.position = windowRect;
                popup.StaysOpen = HasFlag(Event.current.modifiers, EventModifiers.Control);
                popup.ScriptTemplate = composableObject.ElementTemplate;
                popup.Filter = (MonoScript script) =>
                {
                    try
                    {
                        var scriptClass = script.GetClass();
                        if (scriptClass == null) return false;
                        if (scriptClass.IsAbstract) return false;
                        if (scriptClass.GetCustomAttributes(true).OfType<HideFromScriptWindow>().Any()) return false;
                        if (!composableObject.SupportsType(scriptClass)) return false;

                        return true;
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e);
                        return false;
                    }
                };
                popup.Create = createFromScript;

                var IconName = $"TK_{composableObject.GetType().Name}_Icon";
                var icon = AssetDatabase.FindAssets($"t:Texture2D {IconName}", Constants.FindAllFolders)
                             .Select(AssetDatabase.GUIDToAssetPath)
                             .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
                             .FirstOrDefault();

                popup.ScriptIcon = icon;
                popup.ShowPopup();
            }
        }
        public static bool HasFlag(EventModifiers source, EventModifiers flag)
        {
            return (source & flag) == flag;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using ThunderKit.Core.Editor.Controls;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.EditorGUILayout;
using static UnityEditor.EditorGUIUtility;
using ThunderKit.Core.Editor;
using ThunderKit.Core.Attributes;

namespace ThunderKit.Core.Editor
{
    [CustomEditor(typeof(ComposableObject), true)]
    public class ComposableObjectEditor : UnityEditor.Editor
    {
        public class StepData
        {
            public SerializedProperty step;
            public SerializedProperty dataArray;
            public int index;
        }

        List<Type> SupportedTypes;
        SerializedProperty dataArray;
        bool isFolded;
        protected Rect stepRect;
        private void OnEnable()
        {
            try
            {
                var targetObject = target as ComposableObject;

                SupportedTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes()).Where(targetObject.SupportsType).ToList();
            }
            catch
            {
            }
        }

        public override void OnInspectorGUI()
        {
            var composableObject = target as ComposableObject;
            dataArray = serializedObject.FindProperty(nameof(ComposableObject.Data));

            int fieldCount = 0;
            var property = serializedObject.GetIterator();
            bool offset = false;
            if (property != null && property.NextVisible(true))
                do
                {
                    if ("m_script".Equals(property.name, StringComparison.OrdinalIgnoreCase)) continue;
                    if (nameof(ComposableObject.Data).Equals(property.name, StringComparison.OrdinalIgnoreCase)) continue;

                    EditorHelpers.AddField(property);
                    offset = true;
                    fieldCount++;
                }
                while (property.NextVisible(false));
            var bgc = GUI.backgroundColor;

            stepRect = GetControlRect(true, singleLineHeight + standardVerticalSpacing);
            stepRect.x -= 14;
            stepRect.y -= (offset ? 0 : 7);
            stepRect.width += 18;

            for (int i = 0; i < dataArray.arraySize; i++)
            {
                SerializedProperty step = null;
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

                var serializedObject = new SerializedObject(step.objectReferenceValue);
                var stepType = serializedObject.targetObject.GetType();
                var title = ObjectNames.NicifyVariableName(stepType.Name);
                var standardSize = EditorGUI.GetPropertyHeight(step) + standardVerticalSpacing;
                var foldOutRect = new Rect(stepRect.x + 14, stepRect.y + 1, stepRect.width - (standardSize * 2), stepRect.height);

                var iterator = serializedObject.GetIterator().Copy();
                standardSize = EditorGUI.GetPropertyHeight(iterator) + standardVerticalSpacing;
                int stepFieldCount = 0;

                EvaluateProperty(iterator, sp => stepFieldCount++);

                var isSingleLine = stepType.GetCustomAttributes<SingleLineAttribute>().Any() && stepFieldCount == 1;

                stepRect.height += isSingleLine ? 2 : 0;

                GUI.Box(new Rect(stepRect.x, stepRect.y, stepRect.width + 1, stepRect.height), string.Empty);

                if (stepFieldCount > 0 && !isSingleLine)
                    step.isExpanded = EditorGUI.Foldout(foldOutRect, step.isExpanded, title);
                else if (isSingleLine)
                    EvaluateProperty(serializedObject.GetIterator().Copy(), sp =>
                    {
                        var propHeight = EditorGUI.GetPropertyHeight(sp) + standardVerticalSpacing;
                        var stanSize = singleLineHeight + standardVerticalSpacing;
                        EditorHelpers.AddField(new Rect(stepRect.position.x + 14, stepRect.position.y + 2, stepRect.width - (18 + standardSize), propHeight), sp);
                        stepRect.y += propHeight - stanSize;
                    });
                else
                    EditorGUI.LabelField(foldOutRect, title);

                GUI.backgroundColor = new Color(0.8f, 0.0f, 0.0f, 1f);
                Rect deleteRect = new Rect(stepRect.x + 1 + stepRect.width - standardSize, stepRect.y + 3, standardSize, standardSize);
                Rect upRect = new Rect(deleteRect.x - (standardSize + 2), stepRect.y, standardSize, standardSize);
                Rect downRect = new Rect(upRect.x - (standardSize + 2), stepRect.y, standardSize, standardSize);

                var popupIcon = EditorGUIUtility.IconContent("_Popup");
                if (Event.current.type == EventType.Repaint)
                    GUIStyle.none.Draw(deleteRect, popupIcon, false, false, false, false);

                if (Event.current.type == EventType.MouseUp && deleteRect.Contains(Event.current.mousePosition))
                {
                    var menu = new GenericMenu();
                    var stepData = new StepData { step = step, index = i, dataArray = dataArray };
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(nameof(Remove))), false, Remove, stepData);
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(nameof(MoveToTop))), false, MoveToTop, stepData);
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(nameof(MoveUp))), false, MoveUp, stepData);
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(nameof(MoveDown))), false, MoveDown, stepData);
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(nameof(MoveToBottom))), false, MoveToBottom, stepData);
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(nameof(EditScript))), false, EditScript, stepData);
                    menu.ShowAsContext();
                }

                GUI.backgroundColor = bgc;
                if (stepFieldCount > 0 && !isSingleLine && step.isExpanded)
                {
                    stepRect.y += standardVerticalSpacing;
                    EvaluateProperty(serializedObject.GetIterator().Copy(), sp =>
                    {
                        var propHeight = EditorGUI.GetPropertyHeight(sp) + standardVerticalSpacing;
                        var stanSize = singleLineHeight + standardVerticalSpacing;
                        stepRect.y += stanSize;
                        EditorHelpers.AddField(new Rect(stepRect.position.x + 14, stepRect.position.y, stepRect.width - 18, propHeight), sp);
                        stepRect.y += propHeight - stanSize;
                    });
                    stepRect.y += standardVerticalSpacing * 2;
                }

                GUI.backgroundColor = bgc;
                stepRect.y += (singleLineHeight + standardVerticalSpacing) - 1;

                stepRect.height -= isSingleLine ? 2 : 0;
                stepRect.y += isSingleLine ? 2 : 0;
            }

            stepRect.y += singleLineHeight / 2;
            stepRect.width = 230;
            stepRect.height = 23;
            stepRect.x = (currentViewWidth - stepRect.width) / 2;

            OnAddElementGui(composableObject);
        }
        static void EditScript(object data)
        {
            if (data is StepData stepData 
             && stepData.step.objectReferenceValue is ScriptableObject scriptableObject)
                ScriptEditorHelper.EditScript(scriptableObject);
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
            AssetDatabase.RemoveObjectFromAsset(stepData.step.objectReferenceValue);
            stepData.dataArray.DeleteArrayElementAtIndex(stepData.index);
            for (int x = stepData.index; x < stepData.dataArray.arraySize; x++)
                stepData.dataArray.MoveArrayElement(x + 1, x);
            stepData.dataArray.arraySize--;

            stepData.dataArray.serializedObject.SetIsDifferentCacheDirty();
            stepData.dataArray.serializedObject.ApplyModifiedProperties();
        }

        ScriptableObject Create(Type type)
        {
            var instance = CreateInstance(type);
            instance.name = type.Name;

            AssetDatabase.AddObjectToAsset(instance, target);

            var stepField = dataArray.GetArrayElementAtIndex(dataArray.arraySize++);

            stepField.objectReferenceValue = instance;
            stepField.serializedObject.SetIsDifferentCacheDirty();
            stepField.serializedObject.ApplyModifiedProperties();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(instance));
            AssetDatabase.SaveAssets();

            return instance;
        }

        private void OnAddElementGui(ComposableObject composableObject)
        {
            bool Filter(MonoScript script)
            {
                var scriptClass = script.GetClass();
                if (scriptClass == null)
                {
                    return false;
                }
                return !scriptClass.IsAbstract && scriptClass.IsSubclassOf(composableObject.ElementType);
            }
            ScriptableObject CreateFromScript(MonoScript script)
            {
                if (!script) return null;
                if (script.GetClass() == null) return null;

                return Create(script.GetClass());
            }

            AddScriptWindow.Show(stepRect, composableObject.ElementType, CreateFromScript, Filter, composableObject.ElementTemplate);
        }

        protected virtual IEnumerable<Type> UpdateSearch(string searchString) => SupportedTypes.Where(t => t.Name.ToLower().Contains(searchString.ToLower()));

        private static void EvaluateProperty(SerializedProperty property, Action<SerializedProperty> action)
        {
            if (property != null && property.NextVisible(true))
            {
                do
                {
                    if ("m_script".Equals(property.name, System.StringComparison.OrdinalIgnoreCase)) continue;
                    action(property);
                }
                while (property.NextVisible(false));
            }
        }
    }
}
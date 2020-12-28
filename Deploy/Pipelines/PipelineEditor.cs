#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Deploy.Gui;
using PassivePicasso.ThunderKit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using EGL = UnityEditor.EditorGUILayout;
using EGU = UnityEditor.EditorGUIUtility;
using GL = UnityEngine.GUILayout;

namespace PassivePicasso.ThunderKit.Deploy.Pipelines
{
    [CustomEditor(typeof(Pipeline), true)]
    public class PipelineEditor : UnityEditor.Editor
    {
        internal static List<Type> AllTypes;

        List<Type> AvailablePipelineJobs;
        SerializedProperty runSteps;
        TypeSearchSuggest suggestor;

        private void OnEnable()
        {
            var allGlobalTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes()).ToArray();
            AllTypes = allGlobalTypes.Where(t => typeof(PipelineJob).IsAssignableFrom(t)).ToList();

            AvailablePipelineJobs = AllTypes.Where(t =>
            {
                var customAttributes = t.GetCustomAttributes();
                var pipelineSupportAttributes = customAttributes.OfType<PipelineSupportAttribute>();
                var supportsThisPipeline = pipelineSupportAttributes.Any(psa => psa.HandlesPipeline(target.GetType()));

                return supportsThisPipeline;
            }).ToList();
            suggestor = CreateInstance<TypeSearchSuggest>();
            suggestor.OnSuggestionGUI = RenderOption;
            suggestor.Evaluate = UpdateSearch;
        }

        private void OnDisable()
        {
            DestroyImmediate(suggestor);
        }

        public override void OnInspectorGUI()
        {
            runSteps = serializedObject.FindProperty(nameof(Pipeline.runSteps));

            var property = serializedObject.GetIterator();
            if (property != null && property.NextVisible(true))
                do
                {
                    if ("m_script".Equals(property.name, System.StringComparison.OrdinalIgnoreCase)) continue;
                    if (nameof(Pipeline.runSteps).Equals(property.name, System.StringComparison.OrdinalIgnoreCase)) continue;

                    EditorHelpers.AddField(property);
                }
                while (property.NextVisible(false));

            for (int i = 0; i < runSteps.arraySize; i++)
            {
                var step = runSteps.GetArrayElementAtIndex(i);

                void RenderStep(SerializedObject serializedObject)
                {
                    float height = EGU.singleLineHeight;
                    float standardSize = EGU.singleLineHeight + EGU.standardVerticalSpacing;

                    var stepRect = EGL.GetControlRect(true, standardSize);
                    ExecuteField(serializedObject.GetIterator().Copy(), sp =>
                    {
                        height += standardSize;
                        if (sp.isExpanded)
                            height += standardSize * (sp.arraySize + 1);
                    });


                    stepRect = new Rect(stepRect.position.x, stepRect.position.y + EGU.standardVerticalSpacing,
                                    stepRect.width + 2, height + (EGU.standardVerticalSpacing * 3));
                    GUI.Box(stepRect, "");

                    stepRect = new Rect(stepRect.position.x, stepRect.position.y,
                                    stepRect.width, standardSize);

                    var bgc = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.65f, 0.65f, 0.65f, 1f);
                    GUI.Box(stepRect, ObjectNames.NicifyVariableName(serializedObject.targetObject.GetType().Name));

                    stepRect = new Rect(stepRect.x + stepRect.width - standardSize, stepRect.y + 2, standardSize - 2, standardSize - 4);
                    GUI.backgroundColor = new Color(0.8f, 0.0f, 0.0f, 1f);
                    if (GUI.Button(stepRect, "X"))
                    {
                        AssetDatabase.RemoveObjectFromAsset(step.objectReferenceValue);
                        runSteps.DeleteArrayElementAtIndex(i);
                        for (int x = i; x < runSteps.arraySize; x++)
                            runSteps.MoveArrayElement(x + 1, x);
                        runSteps.arraySize--;

                        runSteps.serializedObject.SetIsDifferentCacheDirty();
                        runSteps.serializedObject.ApplyModifiedProperties();
                        //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(runSteps.objectReferenceValue));
                        AssetDatabase.SaveAssets();
                    }

                    GUI.backgroundColor = bgc;

                    EGL.GetControlRect(true, EGU.standardVerticalSpacing);

                    ExecuteField(serializedObject.GetIterator().Copy(), sp => EditorHelpers.AddField(sp));
                }

                if (step.objectReferenceValue)
                    RenderStep(new SerializedObject(step.objectReferenceValue));
            }

            if (suggestor.OnSuggestGUI("Add Pipeline Job"))
                Repaint();
        }

        private IEnumerable<Type> UpdateSearch(string searchString) => AvailablePipelineJobs.Where(t => t.Name.ToLower().Contains(searchString.ToLower()));
        private bool RenderOption(int index, Type option)
        {
            if (GL.Button(option.Name))
            {
                var stepInstance = CreateInstance(option);
                stepInstance.name = option.Name;
                //stepInstance.hideFlags = HideFlags.HideInHierarchy;

                AssetDatabase.AddObjectToAsset(stepInstance, target);

                var stepField = runSteps.GetArrayElementAtIndex(runSteps.arraySize++);

                stepField.objectReferenceValue = stepInstance;
                stepField.serializedObject.SetIsDifferentCacheDirty();
                stepField.serializedObject.ApplyModifiedProperties();

                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stepInstance));
                AssetDatabase.SaveAssets();
                suggestor.Cleanup();
                return true;
            }
            return false;
        }

        private static void ExecuteField(SerializedProperty property, Action<SerializedProperty> action)
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
#endif
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Windows;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Inspectors
{
    [CustomEditor(typeof(Pipeline), true)]
    public class PipelineEditor : ComposableObjectEditor
    {
#if UNITY_2019_1_OR_NEWER
        private const int ButtonHeight = 18;
#elif UNITY_2018_1_OR_NEWER
        private const int ButtonHeight = 15;
#endif
        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
            var pipeline = target as Pipeline;
            var width = 100;
            var rect = new Rect(46, 22, width, 15);
            var cvw = EditorGUIUtility.currentViewWidth;
            var quickAccessContent = new GUIContent("Quick Access");
            var titleContent = new GUIContent(pipeline.name);
            if (cvw < 350)
            {
                var titleContentSize = GUI.skin.button.CalcSize(titleContent);
                rect.x += titleContentSize.x;
                rect.y = 6;
            }

            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            EditorGUI.BeginChangeCheck();
            var quickAccess = GUI.Toggle(rect, settings.QuickAccessPipelines?.Contains(pipeline) ?? false, quickAccessContent);
            if (EditorGUI.EndChangeCheck())
            {
                settings.SetQuickAccess(pipeline, quickAccess);

                serializedObject.ApplyModifiedProperties();
            }

            width = 160;
            var buttonArea = new Rect(cvw - width - 52, 10 + ButtonHeight, width, ButtonHeight + 2);
            GUILayout.BeginArea(buttonArea);
            using (var scope = new GUILayout.HorizontalScope())
            {
                try
                {
                    if (GUILayout.Button("Execute", GUILayout.Height(ButtonHeight)))
                        _ = pipeline.Execute();
                    if (GUILayout.Button("Log", GUILayout.Height(ButtonHeight), GUILayout.Width(50)))
                    {
                        var pipelineLog = AssetDatabase.FindAssets($"t:{nameof(PipelineLog)}")
                                                        .Select(AssetDatabase.GUIDToAssetPath)
                                                        .Where(ap => ap.Contains(pipeline.name))
                                                        .Select(AssetDatabase.LoadAssetAtPath<PipelineLog>)
                                                        .OrderByDescending(log => log.CreatedDate)
                                                        .First();
                        PipelineLogWindow.ShowLog(pipelineLog);
                    }
                }
                catch { }
                finally
                {
                    GUILayout.EndArea();
                }
            }
        }

        protected override Rect OnBeforeElementHeaderGUI(Rect rect, ComposableElement element, ref string title)
        {
            var job = element as PipelineJob;
            var toggleRect = new Rect(rect.x + 2, rect.y + 1, 14, EditorGUIUtility.singleLineHeight);
            var titleContent = new GUIContent(title);
            job.Active = GUI.Toggle(toggleRect, job.Active, titleContent);
            toggleRect.x += 16;
            var size = GUI.skin.label.CalcSize(titleContent);
            toggleRect.width = size.x;
            toggleRect.height = size.y;
            GUI.Label(toggleRect, title);
            title = string.Empty;

            return rect;
        }
        protected override Rect OnAfterElementHeaderGUI(Rect rect, ComposableElement element)
        {
            var offset = 16;
            rect.x += offset;
            rect.width -= offset;
            return rect;
        }
    }
}
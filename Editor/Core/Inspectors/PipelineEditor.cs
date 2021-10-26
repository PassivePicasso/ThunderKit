using System.Linq;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Windows;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Inspectors
{
    [CustomEditor(typeof(Pipeline), true)]
    public class PipelineEditor : ComposableObjectEditor
    {
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
        public override async void OnInspectorGUI()
        {
            var pipeline = target as Pipeline;
            if (!pipeline)
            {
                //this should never get hit?
                Debug.LogError("Drawing PipelineEditor inspector for pipeline that is not targetted.");
                return;
            }
            using(var scope = new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Execute"))
                    await pipeline.Execute();
                if(GUILayout.Button("Show Log"))
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
            GUILayout.Space(4);

            base.OnInspectorGUI();
        }
    }
}
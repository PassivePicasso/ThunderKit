using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace ThunderKit.Core.Editor.Inspectors
{
    [CustomEditor(typeof(Pipeline), true)]
    public class PipelineEditor : ComposableObjectEditor
    {
        protected override Rect PreHeader(Rect rect, ComposableElement element)
        {
            var job = element as PipelineJob;
            var toggleRect = new Rect(rect.x - 14, rect.y + 1, 14, EditorGUIUtility.singleLineHeight);
            rect.x += 16;
            job.Active = GUI.Toggle(toggleRect, job.Active, new GUIContent());

            return rect;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var size = new Vector2(250, 24);
            var rect = GUILayoutUtility.GetRect(size.x, size.y);
            rect.width = size.x;
            rect.y += standardVerticalSpacing * 2;
            rect.x = (currentViewWidth / 2) - (rect.width / 2);
            if (GUI.Button(rect, "Execute"))
            {
                var pipeline = target as Pipeline;
                pipeline?.Execute();
            }
        }
    }
}
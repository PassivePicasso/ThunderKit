using ThunderKit.Core.Editor.Controls;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace ThunderKit.Core.Editor
{
    [CustomEditor(typeof(Pipeline), true)]
    public class PipelineEditor : ComposableObjectEditor
    {
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
using ThunderKit.Core.Attributes;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Editor.Controls
{
    [CustomPropertyDrawer(typeof(ContextAssistAttribute))]
    public class ContextAssistDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

        }
    }
}
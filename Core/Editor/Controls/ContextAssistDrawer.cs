using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Core.Editor.Controls
{
    [CustomPropertyDrawer(typeof(ContextAssistAttribute))]
    public class ContextAssistDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

        }
    }
}
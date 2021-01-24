#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Editor
{
    public static class EditorHelpers
    {
        public static void AddField(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property, true);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.SetIsDifferentCacheDirty();
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        public static void AddField(Rect rect, SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, property, true);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.SetIsDifferentCacheDirty();
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif
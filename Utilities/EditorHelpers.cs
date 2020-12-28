#if UNITY_EDITOR
using UnityEditor;


namespace PassivePicasso.ThunderKit.Utilities
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
    }
}
#endif
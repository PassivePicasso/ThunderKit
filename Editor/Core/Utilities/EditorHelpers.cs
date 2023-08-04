using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Utilities
{
    public static class EditorHelpers
    {
        public static void AddField(SerializedProperty property, string label = null)
        {
            EditorGUI.BeginChangeCheck();
            if (string.IsNullOrEmpty(label))
                EditorGUILayout.PropertyField(property, true);
            else
                EditorGUILayout.PropertyField(property, new GUIContent(label), true);

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.SetIsDifferentCacheDirty();
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        public static void AddField(Rect rect, SerializedProperty property, string label = null)
        {
            EditorGUI.BeginChangeCheck();
            if (string.IsNullOrEmpty(label))
                EditorGUI.PropertyField(rect, property, true);
            else
                EditorGUI.PropertyField(rect, property, new GUIContent(label), true);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.SetIsDifferentCacheDirty();
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        public static void DrawSanitizedPropertiesExcluding(SerializedObject obj,  params string[] propertyToExclude)
        {
            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!propertyToExclude.Contains(iterator.name))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(iterator, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (iterator.propertyType == SerializedPropertyType.String)
                        {
                            iterator.stringValue = new string(iterator.stringValue.Where(x => !char.IsControl(x)).ToArray());
                        }
                        else if (iterator.isArray && iterator.arrayElementType == "string")
                        {
                            for (int i = 0; i < iterator.arraySize; i++)
                            {
                                SerializedProperty prop = iterator.GetArrayElementAtIndex(i);
                                prop.stringValue = new string(prop.stringValue.Where(x => !char.IsControl(x)).ToArray());
                            }
                        }
                    }
                }
            }
        }
    }
}
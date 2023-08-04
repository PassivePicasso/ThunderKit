using System.Linq;
using ThunderKit.Core.Attributes;
using UnityEditor;
using UnityEngine;
using static Markdig.Helpers.StringLineGroup;

namespace ThunderKit.Core
{

    [CustomPropertyDrawer(typeof(PathReferenceResolverAttribute))]
    public class PathReferenceResolverDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUI.PropertyField(position, property, label);

            if (EditorGUI.EndChangeCheck())
            {
                if (property.propertyType == SerializedPropertyType.String)
                {
                    property.stringValue = new string(property.stringValue.Where(x => !char.IsControl(x)).ToArray());
                }
                else if (property.isArray && property.arrayElementType == "string")
                {
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        SerializedProperty prop = property.GetArrayElementAtIndex(i);
                        prop.stringValue = new string(prop.stringValue.Where(x => !char.IsControl(x)).ToArray());
                    }
                }

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.UpdateIfRequiredOrScript();
            }
        }
    }
}
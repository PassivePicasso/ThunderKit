using UnityEngine;
using UnityEditor;
using System.Linq;
using RoR2;
using System.Collections.Generic;

namespace RainOfStages.Editor
{
    [CustomPropertyDrawer(typeof(DirectorCardCategorySelection), true)]
    public class DirectorCardCategorySelectionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            List<string> names = new List<string>();
            DirectorCardCategorySelection[] pickedCategory = null;
            string replacement = "";
            switch (label.text)
            {
                case string name when "Monster Categories".Equals(name):
                    names = DCCSProcessor.MonsterSelections.Select(ms => ms.name).ToList();
                    pickedCategory = DCCSProcessor.MonsterSelections;
                    replacement = "Monsters";
                    break;
                case string name when "Interactable Categories".Equals(name):
                    names = DCCSProcessor.InteractableSelections.Select(ms => ms.name).ToList();
                    pickedCategory = DCCSProcessor.InteractableSelections;
                    replacement = "Interactables";
                    break;
                case string name when "Monster Family Categories".Equals(name):
                    names = DCCSProcessor.MonsterFamilies.Select(ms => ms.name).ToList();
                    pickedCategory = DCCSProcessor.MonsterFamilies;
                    break;
            }
            if (pickedCategory == null)
            {
                base.OnGUI(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.LabelField(position, label);

            position = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

            string currentSelection = property.objectReferenceValue?.name;
            var selectedIndex = names.IndexOf(currentSelection);
            int newIndex = -1;

            if (!string.IsNullOrEmpty(replacement))
                newIndex = EditorGUI.Popup(position, selectedIndex, names.Select(name => ObjectNames.NicifyVariableName(name.Replace(replacement, ""))).ToArray());
            else
                newIndex = EditorGUI.Popup(position, selectedIndex, names.Select(name => ObjectNames.NicifyVariableName(name)).ToArray());

            if (newIndex > -1)
                property.objectReferenceValue = pickedCategory[newIndex];
            else
                property.objectReferenceValue = null;

            EditorGUI.EndProperty();

        }
    }
}
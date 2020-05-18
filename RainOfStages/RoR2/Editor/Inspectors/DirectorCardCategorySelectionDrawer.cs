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
        static string[] CategorySelectionGuids;
        static DirectorCardCategorySelection[] CategorySelections;
        static DirectorCardCategorySelection[] MonsterSelections;
        static DirectorCardCategorySelection[] InteractableSelections;
        static DirectorCardCategorySelection[] MonsterFamilies;

        [InitializeOnLoadMethod()]
        static void Initialize()
        {
            EditorApplication.projectChanged += UpdateSelectionsCache;
            UpdateSelectionsCache();
        }

        private static void UpdateSelectionsCache()
        {
            string[] set = AssetDatabase.FindAssets("t:DirectorCardCategorySelection");
            if (CategorySelectionGuids != null && CategorySelectionGuids.SequenceEqual(set)) return;

            CategorySelectionGuids = set;

            CategorySelections = set.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                                .Select(path => AssetDatabase.LoadAssetAtPath<DirectorCardCategorySelection>(path))
                                .ToArray();

            MonsterSelections = CategorySelections.Where(dccs => dccs.name.Contains("Monsters")).ToArray();
            InteractableSelections = CategorySelections.Where(dccs => dccs.name.Contains("Interactables")).ToArray();
            MonsterFamilies = CategorySelections.Where(dccs => dccs.name.Contains("Family")).ToArray();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            List<string> names = new List<string>();
            DirectorCardCategorySelection[] pickedCategory = null;
            string replacement = "";
            switch (label.text)
            {
                case string name when "Monster Categories".Equals(name):
                    names = MonsterSelections.Select(ms => ms.name).ToList();
                    pickedCategory = MonsterSelections;
                    replacement = "Monsters";
                    break;
                case string name when "Interactable Categories".Equals(name):
                    names = InteractableSelections.Select(ms => ms.name).ToList();
                    pickedCategory = InteractableSelections;
                    replacement = "Interactables";
                    break;
                case string name when "Monster Family Categories".Equals(name):
                    names = MonsterFamilies.Select(ms => ms.name).ToList();
                    pickedCategory = MonsterFamilies;
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

            EditorGUI.EndProperty();

        }
    }
}
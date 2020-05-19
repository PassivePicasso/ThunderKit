using RainOfStages.Proxy;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace RainOfStages.Editor
{
    public class DCCSProcessor
    {
        public static string[] CategorySelectionGuids;
        public static DirectorCardCategorySelection[] CategorySelections;
        public static DirectorCardCategorySelection[] MonsterSelections;
        public static DirectorCardCategorySelection[] InteractableSelections;
        public static DirectorCardCategorySelection[] MonsterFamilies;

        static int waitTime = 10;
        static float elapsed = 0;

        private static void UpdateSelectionsCache()
        {
            string[] set = AssetDatabase.FindAssets("t:DirectorCardCategorySelection");

            elapsed += Time.deltaTime;
            if (elapsed < waitTime) return;
            elapsed = 0;

            CategorySelectionGuids = set;

            CategorySelections = set.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                                .Select(path => AssetDatabase.LoadAssetAtPath<DirectorCardCategorySelection>(path))
                                .ToArray();

            MonsterSelections = CategorySelections.Where(dccs => dccs.name.Contains("Monsters")).ToArray();
            InteractableSelections = CategorySelections.Where(dccs => dccs.name.Contains("Interactables")).ToArray();
            MonsterFamilies = CategorySelections.Where(dccs => dccs.name.Contains("Family")).ToArray();
        }

        [InitializeOnLoadMethod()]
        static void Initialize()
        {
            EditorApplication.update += UpdateSelectionsCache;
            UpdateSelectionsCache();
        }
    }
}
using RainOfStages.Proxies;
using RainOfStages.Stage;
using RoR2;
using RoR2.Navigation;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RainOfStages.Configurator
{
    public class MapConfigurator : MonoBehaviour
    {
        public float airHeight;
        public float spacing;
        public HullClassification debugHullDef;

        private GameObject sceneInfoObject;
        private SceneInfo sceneInfo;

        public int InteractableCoins = 300;
        public int MonsterCoins = 300;
        public CategoryProxy[] interactableCategories;
        public CategoryProxy[] monsterCategories;

        /*Proxy*/
        public NodeGraphProxy airNodeGraph;
        /*Proxy*/
        public NodeGraphProxy groundNodeGraph;

        private void Awake()
        {
            var sceneInfoType = typeof(SceneInfo);
            var stageInfoType = typeof(ClassicStageInfo);
            var airNodesAssetField = sceneInfoType.GetField("airNodesAsset", BindingFlags.NonPublic | BindingFlags.Instance);
            var groundNodesAssetField = sceneInfoType.GetField("groundNodesAsset", BindingFlags.NonPublic | BindingFlags.Instance);
            var monsterCategoriesField = stageInfoType.GetField("monsterCategories", BindingFlags.NonPublic | BindingFlags.Instance);
            var interactableCategoriesField = stageInfoType.GetField("interactableCategories", BindingFlags.NonPublic | BindingFlags.Instance);

            sceneInfoObject = new GameObject("SceneInfo");
            sceneInfoObject.SetActive(false);
            Debug.Log("Created Inactive GameObject");

            sceneInfo = sceneInfoObject.AddComponent<SceneInfo>();
            Debug.Log("Added SceneInfo Component");

            SetValue(groundNodesAssetField, sceneInfo, (NodeGraph)groundNodeGraph);
            SetValue(airNodesAssetField, sceneInfo, (NodeGraph)airNodeGraph);

            sceneInfo.groundNodeGroup = gameObject.AddComponent<MapNodeGroup>();
            sceneInfo.groundNodeGroup.nodeGraph = groundNodeGraph;
            sceneInfo.airNodeGroup = gameObject.AddComponent<MapNodeGroup>();
            sceneInfo.airNodeGroup.nodeGraph = airNodeGraph;

            var classicStageInfo = sceneInfoObject.AddComponent<ClassicStageInfo>();
            Debug.Log("Added ClassicStageInfo Component");

            if (interactableCategories != null && interactableCategories.Length > 0)
                SetValue(interactableCategoriesField, classicStageInfo, ConvertProxy(interactableCategories));

            if (monsterCategories != null && monsterCategories.Length > 0)
                SetValue(monsterCategoriesField, classicStageInfo, ConvertProxy(monsterCategories));

            classicStageInfo.sceneDirectorInteractibleCredits = InteractableCoins;
            classicStageInfo.sceneDirectorMonsterCredits = MonsterCoins;
            Debug.Log("Set Credits");

            sceneInfoObject.SetActive(true);
            Debug.Log("Activated SceneInfo GameObject");
        }

        private void SetValue<T>(FieldInfo fieldInfo, object target, T value)
        {
            fieldInfo.SetValue(target, value);
            Debug.Log($"Assigned {fieldInfo.DeclaringType.Name}.{fieldInfo.Name}");
        }

        private DirectorCardCategorySelection ConvertProxy(CategoryProxy[] categoryProxies)
        {
            var interactables = ScriptableObject.CreateInstance<DirectorCardCategorySelection>();
            interactables.categories = categoryProxies.Select(cp => new DirectorCardCategorySelection.Category()
            {
                name = cp.name,
                cards = cp.cards.Select(card => card.ToDirectorCard()).ToArray(),
                selectionWeight = cp.selectionWeight
            }).ToArray();
            return interactables;
        }
    }
}
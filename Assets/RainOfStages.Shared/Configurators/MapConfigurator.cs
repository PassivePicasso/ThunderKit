using RainOfStages.Proxies;
using RainOfStages.Stage;
using RoR2;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace RainOfStages.Configurator
{
    public class MapConfigurator : MonoBehaviour
    {
        public float airHeight;
        public float spacing;

        private GameObject sceneInfoObject;
        private SceneInfo sceneInfo;
        public Material DebugMaterial;

        public int InteractableCoins = 200;
        public int MonsterCoins = 0;

        public GameObject teleporterInstance;
        public DirectorCardProxy teleporterCardProxy;
        public WaveIntervalSetting waveIntervalSetting;

        public CategoryProxy[] interactableCategories;
        public CategoryProxy[] monsterCategories;

        public NodeGraphProxy airNodeGraphProxy;
        public NodeGraphProxy groundNodeGraphProxy;

        public Stage.NodeGraph airNodeGraph { get; set; }
        public Stage.NodeGraph groundNodeGraph { get; set; }

        private void OnEnable()
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

            SetValue(groundNodesAssetField, sceneInfo, (RoR2.Navigation.NodeGraph)groundNodeGraphProxy);
            SetValue(airNodesAssetField, sceneInfo, (RoR2.Navigation.NodeGraph)airNodeGraphProxy);

            sceneInfo.groundNodeGroup = gameObject.AddComponent<RoR2.Navigation.MapNodeGroup>();
            sceneInfo.groundNodeGroup.nodeGraph = groundNodeGraphProxy;
            sceneInfo.groundNodeGroup.graphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;

            sceneInfo.airNodeGroup = gameObject.AddComponent<RoR2.Navigation.MapNodeGroup>();
            sceneInfo.airNodeGroup.nodeGraph = airNodeGraphProxy;
            sceneInfo.airNodeGroup.graphType = RoR2.Navigation.MapNodeGroup.GraphType.Air;

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
            
            ConfigureGlobalEventManager();
            ConfigureSceneDirector();
        }

        private GameObject DebugNodeGraph;
        private int hullIndex = -1;
        private MeshFilter debugMeshFilter;
        bool AirNodes = false;

        private RoR2.Navigation.NodeGraph theSource => AirNodes ? sceneInfo.airNodes : sceneInfo.groundNodes;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7))
                AirNodes = !AirNodes;

            if (Input.GetKeyDown(KeyCode.F8))
            {
                hullIndex++;
                if ((RoR2.HullClassification)hullIndex == RoR2.HullClassification.Count)
                {
                    hullIndex = -1;
                    if (DebugNodeGraph)
                        DebugNodeGraph.SetActive(false);

                    return;
                }

                var mesh = theSource.GenerateLinkDebugMesh((RoR2.HullClassification)hullIndex);
                if (DebugNodeGraph)
                {
                    DebugNodeGraph.SetActive(true);
                    debugMeshFilter = DebugNodeGraph.GetComponent<MeshFilter>();
                    debugMeshFilter.mesh = mesh;
                }
                else
                {
                    DebugNodeGraph = new GameObject("DebugNodeGraph", typeof(MeshFilter), typeof(MeshRenderer));
                    DebugNodeGraph.transform.position = Vector3.zero;
                    debugMeshFilter = DebugNodeGraph.GetComponent<MeshFilter>();
                    debugMeshFilter.mesh = mesh;
                    var renderer = DebugNodeGraph.GetComponent<MeshRenderer>();
                    renderer.material = DebugMaterial;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            var old = Gizmos.color;

            Gizmos.color = Color.red;
            if (airNodeGraph != null)
                foreach (var node in airNodeGraph.nodes)
                    Gizmos.DrawCube(node.position, Vector3.one * 0.1f);

            Gizmos.color = Color.yellow;
            if (groundNodeGraph != null)
                foreach (var node in groundNodeGraph.nodes)
                    Gizmos.DrawCube(node.position, Vector3.one * 0.1f);

            Gizmos.color = old;
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

        private void ConfigureSceneDirector()
        {
            if (NetworkServer.active)
            {
                var directorGameObject = new GameObject("GameDirector");

                float[] minIntervals = waveIntervalSetting.moneyWaveIntervalsMin;
                float[] maxIntervals = waveIntervalSetting.moneyWaveIntervalsMax;

                if (minIntervals.Length != maxIntervals.Length)
                    throw new System.Exception("wave interval min max values are not the same length.");

                RoR2.RangeFloat[] intervals = new RangeFloat[minIntervals.Length];
                for (int i = 0; i < minIntervals.Length; i++)
                {
                    var min = minIntervals[i];
                    var max = maxIntervals[i];
                    intervals[i] = new RangeFloat { min = min, max = max };
                }

                directorGameObject.SetActive(false);

                var directorCore = directorGameObject.AddComponent<DirectorCore>();
                var sceneDirector = directorGameObject.AddComponent<SceneDirector>();

                sceneDirector.teleporterInstance = teleporterInstance;
                sceneDirector.teleporterSpawnCard = teleporterCardProxy.ToDirectorCard().spawnCard;

                var combatDirectorA = directorGameObject.AddComponent<CombatDirector>();
                combatDirectorA.moneyWaveIntervals = intervals;


                var combatDirectorB = directorGameObject.AddComponent<CombatDirector>();
                combatDirectorB.moneyWaveIntervals = intervals;

                directorGameObject.SetActive(true);
            }
            Debug.Log($"Completed {nameof(ConfigureSceneDirector)}");
        }

        private void ConfigureGlobalEventManager()
        {
            if (NetworkServer.active)
            {
                var gem = gameObject.AddComponent<RoR2.GlobalEventManager>();
                gem.AACannonMuzzleEffect = (GameObject)Resources.Load("prefabs/effects/muzzleflashes/muzzleflashaacannon");
                gem.AACannonPrefab = (GameObject)Resources.Load("prefabs/projectiles/aacannon");
                gem.chainLightingPrefab = (GameObject)Resources.Load("prefabs/projectiles/chainlightning");
                gem.daggerPrefab = (GameObject)Resources.Load("prefabs/projectiles/daggerprojectile");
                gem.explodeOnDeathPrefab = (GameObject)Resources.Load("prefabs/networkedobjects/willowispdelay");
                gem.healthOrbPrefab = (GameObject)Resources.Load("prefabs/networkedobjects/healthglobe");
                gem.missilePrefab = (GameObject)Resources.Load("prefabs/projectiles/missileprojectile");
                gem.plasmaCorePrefab = (GameObject)Resources.Load("prefabs/projectiles/plasmacore");
            }
            Debug.Log($"Completed {nameof(ConfigureGlobalEventManager)}");
        }
    }
}
using RainOfStages.Configurators;
using RainOfStages.Proxies;
using RoR2;
using RoR2.Navigation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;

namespace RainOfStages.Configurator
{
    [RequireComponent(typeof(ClassicStageInfo))]
    public class MapConfigurator : MonoBehaviour
    {
        public float airHeight;
        public float spacing;
        public HullClassification debugHullDef;

        public Transform World;

        private MapNodeGroup groundMapNodeGroup;
        private MapNodeGroup airMapNodeGroup;
        private GameObject sceneInfoObject;
        private SceneInfo sceneInfo;
        private GameObject debugMesh;

        public int InteractableCoins = 300;
        public int MonsterCoins = 300;
        public CategoryProxy[] interactableCategories;
        public CategoryProxy[] monsterCategories;

        public string[] destinations;

        private void Awake()
        {
            var groundNodeObject = new GameObject();
            groundMapNodeGroup = groundNodeObject.AddComponent<MapNodeGroup>();
            var airNodeObject = new GameObject();
            airMapNodeGroup = airNodeObject.AddComponent<MapNodeGroup>();

            ConfigureMapNodeGroup(groundMapNodeGroup, MapNodeGroup.GraphType.Ground);
            ConfigureAirNodes();

            ConfigureSceneInfo();

            debugMesh = new GameObject("DebugMeshDrawer", typeof(MeshFilter), typeof(MeshRenderer));

            var filter = debugMesh.GetComponent<MeshFilter>();
            filter.mesh = humanMesh;
            var renderer = debugMesh.GetComponent<MeshRenderer>();
            renderer.material = navMeshDebugMaterial;
            debugMesh.transform.parent = World;
            debugMesh.transform.localPosition = Vector3.zero;
            debugMesh.SetActive(false);

            //this.enabled = false;

            //Destroy(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                if (debugMesh)
                    debugMesh.SetActive(!debugMesh.activeSelf);
            }
        }

        public float AgentHeight;
        public float AgentRadius;
        public float AgentSlope;
        public float AgentClimb;
        public float VoxelSize;
        public int TileSize;

        private NavMeshTriangulation triangulation;
        public Material navMeshDebugMaterial, navGizmoMaterial;
        public bool NavMeshGenerated;
        private GameObject debugObject;

        NavMeshBuildSettings settings => new NavMeshBuildSettings
        {
            agentClimb = AgentClimb,
            agentHeight = AgentHeight,
            agentRadius = AgentRadius,
            agentSlope = AgentSlope,
            overrideTileSize = true,
            overrideVoxelSize = true,
            voxelSize = VoxelSize,
            tileSize = TileSize,
        };

        public void GenerateTriangulation()
        {
            NavMesh.RemoveAllNavMeshData();
            var markups = new List<NavMeshBuildMarkup>();
            var sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(World, LayerMask.GetMask("World"), NavMeshCollectGeometry.RenderMeshes, 0, markups, sources);
            var renderers = World.gameObject.GetComponentsInChildren<MeshRenderer>();
            var bounds = renderers.Select(r => r.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
            var nvd = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, World.position, World.rotation);
            NavMesh.AddNavMeshData(nvd);
            triangulation = NavMesh.CalculateTriangulation();
            NavMesh.RemoveAllNavMeshData();
        }

        Mesh humanMesh;

        //private void AddLink(MapNode nodeB, float distanceScore, float minJumpHeight, HullClassification hullClassification)
        private void ConfigureMapNodeGroup(MapNodeGroup mapNodeGroup, MapNodeGroup.GraphType graphType)
        {
            var innerWatch = new Stopwatch();
            innerWatch.Start();
            mapNodeGroup.debugHullDef = debugHullDef;
            mapNodeGroup.graphType = graphType;

            var nodeGraph = ScriptableObject.CreateInstance<NodeGraph>();

            GenerateTriangulation();

            var meshes = World.GetComponentsInChildren<MeshFilter>()
                .Where(mf => mf.gameObject.layer == LayerMask.NameToLayer("World"))
                .Select(mf => mf.sharedMesh)
                .ToArray();

            const float fudgeFactor = 100;
            var finalSet = new HashSet<Vector3Int>(
                triangulation.indices
                    .Select(i => triangulation.vertices[i])
                    .Select(vertex => new Vector3Int((int)(vertex.x * fudgeFactor), (int)(vertex.y * fudgeFactor), (int)(vertex.z * fudgeFactor)))
                );

            foreach (Vector3 vertex in finalSet.Select(v => ((Vector3)v) / fudgeFactor))
            {
                if (meshes.Any(m => m.IsPointInside(vertex))) continue;

                mapNodeGroup.AddNode(vertex);
            }

            mapNodeGroup.UpdateNoCeilingMasks();
            mapNodeGroup.UpdateTeleporterMasks();
            mapNodeGroup.Bake(nodeGraph);


            //var bitmask = StartUp.CalculateLineOfSight(nodes);
            //nodeGraph.SetNodes(nodes.AsReadOnly(), bitmask);
            mapNodeGroup.nodeGraph = nodeGraph;

            humanMesh = nodeGraph.GenerateLinkDebugMesh(HullClassification.Human);

            innerWatch.Stop();

            Debug.Log($"Baked {nodeGraph.GetNodeCount()} ground nodes in {innerWatch.ElapsedMilliseconds}ms");
        }

        private void ConfigureAirNodes()
        {
            var mapNodeGroup = airMapNodeGroup;
            var innerWatch = new Stopwatch();
            innerWatch.Start();
            mapNodeGroup.debugHullDef = debugHullDef;
            mapNodeGroup.graphType = MapNodeGroup.GraphType.Air;

            var nodeGraph = ScriptableObject.CreateInstance<NodeGraph>();

            

            var meshes = World.GetComponentsInChildren<MeshFilter>()
                              .Where(mf => mf.gameObject.layer == LayerMask.NameToLayer("World"))
                              .Select(mf => mf.sharedMesh)
                              .ToArray();

            var groundPos = groundMapNodeGroup.GetNodes().Select(n => n.transform.position).ToList();
            const float displacement = 5;
            var airPos = Enumerable.Range(1, 3).SelectMany(i => groundPos.Select(p => p + i*displacement*Vector3.up));

            const float fudgeFactor = 100;
            var finalSet = new HashSet<Vector3Int>(airPos.Select(
                                                       vertex => new Vector3Int(
                                                           (int) (vertex.x*fudgeFactor), 
                                                           (int) (vertex.y*fudgeFactor),
                                                           (int) (vertex.z*fudgeFactor))));

            foreach (Vector3 vertex in finalSet.Select(v => ((Vector3)v) / fudgeFactor))
            {
                if (meshes.Any(m => m.IsPointInside(vertex))) continue;

                mapNodeGroup.AddNode(vertex);
            }

            mapNodeGroup.Bake(nodeGraph);


            //var bitmask = StartUp.CalculateLineOfSight(nodes);
            //nodeGraph.SetNodes(nodes.AsReadOnly(), bitmask);
            mapNodeGroup.nodeGraph = nodeGraph;

            humanMesh = nodeGraph.GenerateLinkDebugMesh(HullClassification.Human);

            innerWatch.Stop();

            Debug.Log($"Baked {nodeGraph.GetNodeCount()} air nodes in {innerWatch.ElapsedMilliseconds}ms");
        }

        #region deprecated code

        //var paramTypes = new Type[] { typeof(MapNode), typeof(float), typeof(float), typeof(HullClassification) };

        //var addlink = typeof(MapNode).GetMethod("AddLink", BindingFlags.NonPublic | BindingFlags.Instance);
        //Debug.Log($"{(string.IsNullOrEmpty(addlink?.Name) ? "Couldn't find" : "Found")} addLink method");

        //const int nodeBIndex = 0;
        //const int distanceScoreIndex = 1;
        //const int minJumpHeightIndex = 2;
        //const int hullClassIndex = 3;
        //object[] parameters = new object[4];
        //for (int ia = 0, ib = 1, ic = 2; ic < triangulation.indices.Length; ia++, ib++, ic++)
        //{
        //    var na = nodes[ia];
        //    var nb = nodes[ib];
        //    var nc = nodes[ic];

        //    parameters[nodeBIndex] = nb;
        //    parameters[distanceScoreIndex] = Mathf.Sqrt((nb.transform.position - na.transform.position).sqrMagnitude);
        //    parameters[hullClassIndex] = HullClassification.Human;
        //    parameters[minJumpHeightIndex] = 1f;
        //    addlink.Invoke(na, parameters);

        //    parameters[nodeBIndex] = nc;
        //    parameters[distanceScoreIndex] = Mathf.Sqrt((nc.transform.position - nb.transform.position).sqrMagnitude);
        //    addlink.Invoke(nb, parameters);

        //    parameters[nodeBIndex] = na;
        //    parameters[distanceScoreIndex] = Mathf.Sqrt((na.transform.position - nc.transform.position).sqrMagnitude);
        //    addlink.Invoke(nc, parameters);
        //}

        //innerWatch.Stop();
        //Debug.Log($"Found and prepared {triangulation.vertices.Length} potential ground nodes in {innerWatch.ElapsedMilliseconds}ms");

        #endregion

        public MapNode AddNode(Vector3 position)
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.position = position;
            gameObject.transform.parent = this.transform;
            var mapNode = gameObject.AddComponent<MapNode>();
            gameObject.name = "MapNode";

            return mapNode;
        }

        private void ConfigureSceneInfo()
        {
            sceneInfoObject = new GameObject("SceneInfo");
            Debug.Log("Created SceneInfo GameObject");
            sceneInfoObject.SetActive(false);
            Debug.Log("Deactivated SceneInfo GameObject");


            sceneInfo = sceneInfoObject.AddComponent<SceneInfo>();
            Debug.Log("Added SceneInfo Component");
            sceneInfo.groundNodeGroup = groundMapNodeGroup;
            Debug.Log("Assigned SceneInfo.groundNodeGroup");
            sceneInfo.groundNodes = sceneInfo.groundNodeGroup.nodeGraph;
            Debug.Log("Assigned SceneInfo.groundNodes");

            sceneInfo.airNodeGroup = airMapNodeGroup;
            Debug.Log("Assigned SceneInfo.airNodeGroup");
            sceneInfo.airNodes = sceneInfo.airNodeGroup.nodeGraph;
            Debug.Log("Assigned SceneInfo.airNodes");

            //sceneInfo.railNodeGroup = railMapNodeGroup;
            //Debug.Log("Assigned SceneInfo.railNodeGroup");
            //sceneInfo.railNodes = sceneInfo.railNodeGroup.nodeGraph;
            //Debug.Log("Assigned SceneInfo.railNodes");

            ConfigureClassicStageInfo();
            Debug.Log("Configured ClassicStageInfo");

            sceneInfoObject.SetActive(true);
            Debug.Log("Activated SceneInfo GameObject");
        }

        private void OnDrawGizmos()
        {
            if (!NavMeshGenerated)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                GenerateTriangulation();

                var vertices = triangulation.vertices;
                var indices = triangulation.indices;

                var mesh = new Mesh();
                mesh.name = "DebugNavMesh";
                mesh.vertices = vertices;
                mesh.triangles = indices;

                if (debugObject) DestroyImmediate(debugObject);

                debugObject = new GameObject("debugObject", typeof(MeshFilter), typeof(MeshRenderer));
                debugObject.hideFlags = HideFlags.DontSave | HideFlags.DontSaveInBuild | HideFlags.HideInInspector;
                debugObject.transform.parent = World;
                debugObject.transform.localPosition = Vector3.zero;

                var meshFilter = debugObject.GetComponent<MeshFilter>();
                var renderer = debugObject.GetComponent<MeshRenderer>();

                meshFilter.mesh = mesh;
                renderer.material = navGizmoMaterial;

                NavMeshGenerated = true;

                stopwatch.Stop();
                Debug.Log($"NavMesh Generated in {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        // Use this for initialization
        void ConfigureClassicStageInfo()
        {
            var classicStageInfo = sceneInfoObject.AddComponent<ClassicStageInfo>();
            Debug.Log("Added ClassicStageInfo Component");

            var classicStageInfoType = typeof(ClassicStageInfo);
            var interactableCategoriesField = classicStageInfoType.GetField("interactableCategories", BindingFlags.NonPublic | BindingFlags.Instance);
            var monsterCategoriesField = classicStageInfoType.GetField("monsterCategories", BindingFlags.NonPublic | BindingFlags.Instance);


            Debug.Log("Reflected field assignments");

            if (interactableCategories != null && interactableCategories.Length > 0)
                interactableCategoriesField.SetValue(classicStageInfo, ConvertProxy(interactableCategories));
            Debug.Log("Created concrete interactable cards from proxies");

            if (monsterCategories != null && monsterCategories.Length > 0)
                monsterCategoriesField.SetValue(classicStageInfo, ConvertProxy(monsterCategories));

            Debug.Log("Created concrete monster cards from proxies");

            Debug.Log("Loaded destinations");

            classicStageInfo.sceneDirectorInteractibleCredits = InteractableCoins;
            classicStageInfo.sceneDirectorMonsterCredits = MonsterCoins;
            Debug.Log("Set Credits");
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
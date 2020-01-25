using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using RainOfStages.Configurator;
using RainOfStages.Stage;
using GraphType = RainOfStages.Stage.MapNodeGroup.GraphType;
using MapNodeGroup = RainOfStages.Stage.MapNodeGroup;
using RainOfStages.Configurators;

namespace RainOfStages
{
    public class NodeGraphBaker : ScriptableObject
    {
        [MenuItem("Tools/Rain of Stages/Bake NodeGraph")]
        public static void Bake()
        {
            var baker = ScriptableObject.CreateInstance<NodeGraphBaker>();
            baker.Build();
            DestroyImmediate(baker);
        }

        public float AgentHeight = 2;
        public float AgentRadius = 2;
        public float AgentSlope = 45;
        public float AgentClimb = 1;
        public float VoxelSize = 0.5f;
        public int TileSize = 20;

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

        public void Build()
        {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            var mapConfigurator = rootObjects.FirstOrDefault(ro => ro.GetComponent<MapConfigurator>() != null).GetComponent<MapConfigurator>();

            Undo.RecordObject(mapConfigurator, "Bake NodeGraph");


            var world = rootObjects.FirstOrDefault(ro => ro.layer == LayerMask.NameToLayer("World"));

            if (world == null)
            {
                UnityEngine.Debug.LogError("Bake Failed: Root GameObject on layer World not found");
                return;
            }

            var meshFilters = world.GetComponentsInChildren<MeshFilter>();

            var triangulation = GenerateTriangulation(world.transform);

            var meshes = meshFilters.Where(mf => mf.gameObject.layer == LayerMask.NameToLayer("World"))
                                    .Select(mf => mf.sharedMesh)
                                    .ToArray();

            const float fudgeFactor = 1000;
            var nodeRootPositions =
                new HashSet<Vector3>(
                    triangulation.indices.Select(i =>
                    {
                        var vertex = triangulation.vertices[i];


                        //else
                        return new Vector3(((int)(vertex.x * fudgeFactor)) / fudgeFactor,
                                           ((int)(vertex.y * fudgeFactor)) / fudgeFactor,
                                           ((int)(vertex.z * fudgeFactor)) / fudgeFactor);
                    })
                );

            var airNodes = Enumerable.Range(1, 3).SelectMany(i => nodeRootPositions.Select(p => p + (i * 5 * Vector3.up))).ToArray();
            var groundNodes = nodeRootPositions.ToList();

            var airNodeGroup = BakeGraph(mapConfigurator, meshes, airNodes, GraphType.Air, $"{GraphType.Air}");
            var groundNodeGroup = BakeGraph(mapConfigurator, meshes, groundNodes, GraphType.Ground, $"{GraphType.Ground}");
            DestroyImmediate(groundNodeGroup);
            DestroyImmediate(airNodeGroup);

            EditorApplication.MarkSceneDirty();
        }

        private GameObject BakeGraph(MapConfigurator mapConfigurator, Mesh[] meshes, IEnumerable<Vector3> positions, GraphType graphType, string name)
        {
            var nodeGraphObject = new GameObject();
            var mapNodeGroup = nodeGraphObject.AddComponent<MapNodeGroup>();

            positions.Where(vertex => !meshes.Any(m => m.IsPointInside(vertex))).ToList().ForEach(mapNodeGroup.AddNode);

            var nodeGraph = ConfigureMapNodeGroup(mapConfigurator, mapNodeGroup, graphType, name);

            NodeGraphProxy nodeGraphProxy = nodeGraph;

            SaveGraph(nodeGraphProxy, $"{name}NodeGraph");

            if (name == $"{MapNodeGroup.GraphType.Ground}")
            {
                mapConfigurator.groundNodeGraphProxy = nodeGraphProxy;
                mapConfigurator.groundNodeGraph = nodeGraph;
            }
            else
            {
                mapConfigurator.airNodeGraphProxy = nodeGraphProxy;
                mapConfigurator.airNodeGraph = nodeGraph;
            }

            return nodeGraphObject;
        }

        NavMeshTriangulation GenerateTriangulation(Transform world)
        {
            NavMesh.RemoveAllNavMeshData();
            var markups = new List<NavMeshBuildMarkup>();
            var sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(world, LayerMask.GetMask("World"), NavMeshCollectGeometry.RenderMeshes, 0, markups, sources);
            var renderers = world.gameObject.GetComponentsInChildren<MeshRenderer>();
            var bounds = renderers.Select(r => r.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
            var nvd = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, world.position, world.rotation);
            NavMesh.AddNavMeshData(nvd);
            var triangulation = NavMesh.CalculateTriangulation();
            NavMesh.RemoveAllNavMeshData();
            return triangulation;
        }

        NodeGraph ConfigureMapNodeGroup(MapConfigurator mapConfigurator, MapNodeGroup mapNodeGroup, GraphType graphType, string name)
        {
            var innerWatch = new Stopwatch();
            innerWatch.Start();

            var nodeGraph = ScriptableObject.CreateInstance<NodeGraph>();
            nodeGraph.name = $"{name}NodeGraph";

            mapNodeGroup.graphType = graphType;
            mapNodeGroup.name = $"{name}MapNodeGroup";


            mapNodeGroup.UpdateNoCeilingMasks();
            mapNodeGroup.UpdateTeleporterMasks();

            mapNodeGroup.Bake(nodeGraph);

            innerWatch.Stop();

            UnityEngine.Debug.Log($"Baked {nodeGraph.nodes.Length} {name} nodes in {innerWatch.ElapsedMilliseconds}ms");

            return nodeGraph;
        }

        void SaveGraph<T>(T nodeGraph, string name) where T : ScriptableObject
        {
            var activeScene = SceneManager.GetActiveScene();
            var scenePath = activeScene.path;

            scenePath = System.IO.Path.GetDirectoryName(scenePath);
            nodeGraph.name = $"{activeScene.name}_{name}";

            AssetDatabase.CreateAsset(nodeGraph, System.IO.Path.Combine(scenePath, $"{nodeGraph.name}.asset"));
        }
    }
}
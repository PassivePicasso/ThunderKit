using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using RoR2;
using System.Collections.Generic;
using RoR2.Navigation;
using System.Linq;
using RainOfStages.Configurators;
using System.Diagnostics;
using RainOfStages.Configurator;
using RainOfStages.Stage;

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
            var groundNodes = new HashSet<Vector3>(
                triangulation.vertices.Select(vertex => new Vector3((int)(vertex.x * fudgeFactor), (int)(vertex.y * fudgeFactor), (int)(vertex.z * fudgeFactor)))
                ).ToList();

            const float displacementValue = 5;
            var displacement = displacementValue * Vector3.up;
            var airNodes = Enumerable.Range(0, 3).SelectMany(i => groundNodes.Select(p => p + (i * displacement)));

            BakeGraph(mapConfigurator, meshes, fudgeFactor, groundNodes, MapNodeGroup.GraphType.Ground);

            BakeGraph(mapConfigurator, meshes, fudgeFactor, airNodes, MapNodeGroup.GraphType.Air);
        }

        private void BakeGraph(MapConfigurator mapConfigurator, Mesh[] meshes, float fudgeFactor, IEnumerable<Vector3> positions, MapNodeGroup.GraphType graphType)
        {
            var nodeGraphObject = new GameObject();
            var mapNodeGroup = nodeGraphObject.AddComponent<MapNodeGroup>();

            positions.Select(v => v / fudgeFactor)
                            .Where(vertex => !meshes.Any(m => m.IsPointInside(vertex)))
                            .ToList()
                            .ForEach(mapNodeGroup.AddNode);

            var nodeGraph = ConfigureMapNodeGroup(mapNodeGroup, MapNodeGroup.GraphType.Ground, HullClassification.Human);
            mapConfigurator.groundNodeGraph = nodeGraph;

            SaveGraph(nodeGraph, $"{graphType}NodeGraph");

            DestroyImmediate(nodeGraphObject);
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

        NodeGraphProxy ConfigureMapNodeGroup(MapNodeGroup mapNodeGroup, MapNodeGroup.GraphType graphType, HullClassification hullClassification)
        {
            var innerWatch = new Stopwatch();
            innerWatch.Start();

            var nodeGraph = ScriptableObject.CreateInstance<NodeGraph>();
            nodeGraph.name = $"{graphType}NodeGraph";

            mapNodeGroup.debugHullDef = hullClassification;
            mapNodeGroup.graphType = graphType;
            mapNodeGroup.name = $"{graphType}MapNodeGroup";
            mapNodeGroup.UpdateNoCeilingMasks();
            if (graphType == MapNodeGroup.GraphType.Ground)
                mapNodeGroup.UpdateTeleporterMasks();

            mapNodeGroup.Bake(nodeGraph);

            innerWatch.Stop();

            UnityEngine.Debug.Log($"Baked {nodeGraph.GetNodeCount()} {graphType} nodes in {innerWatch.ElapsedMilliseconds}ms");

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
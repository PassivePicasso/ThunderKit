using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using GraphType = RoR2.Navigation.MapNodeGroup.GraphType;
using MapNodeGroup = RoR2.Navigation.MapNodeGroup;
using RainOfStages.Configurators;
using SceneInfo = RainOfStages.Proxy.SceneInfo;

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
            var sceneInfo = rootObjects.FirstOrDefault(ro => ro.GetComponent<SceneInfo>() != null).GetComponent<SceneInfo>();

            Undo.RecordObject(sceneInfo, "Bake NodeGraph");

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

            var airNodeGroup = BakeGraph(sceneInfo, meshes, airNodes, GraphType.Air, $"{GraphType.Air}");
            var groundNodeGroup = BakeGraph(sceneInfo, meshes, groundNodes, GraphType.Ground, $"{GraphType.Ground}");
            DestroyImmediate(groundNodeGroup);
            DestroyImmediate(airNodeGroup);

            EditorApplication.MarkSceneDirty();
        }

        private GameObject BakeGraph(SceneInfo sceneInfo, Mesh[] meshes, IEnumerable<Vector3> positions, GraphType graphType, string name)
        {
            var nodeGraphObject = new GameObject();
            var mapNodeGroup = nodeGraphObject.AddComponent<MapNodeGroup>();

            positions.Where(vertex => !meshes.Any(m => m.IsPointInside(vertex))).ToList().ForEach(mapNodeGroup.AddNode);

            var nodeGraph = ConfigureMapNodeGroup(mapNodeGroup, graphType, name);

            SaveGraph(nodeGraph, $"{name}NodeGraph");

            var so = new SerializedObject(sceneInfo);
            if (name == $"{MapNodeGroup.GraphType.Ground}")
                so.FindProperty("groundNodesAsset").objectReferenceValue = nodeGraph;
            else
                so.FindProperty("airNodesAsset").objectReferenceValue = nodeGraph;

            so.ApplyModifiedProperties();

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

        RoR2.Navigation.NodeGraph ConfigureMapNodeGroup(MapNodeGroup mapNodeGroup, GraphType graphType, string name)
        {
            var innerWatch = new Stopwatch();
            innerWatch.Start();

            var nodeGraph = CreateInstance<Proxy.NodeGraph>();
            nodeGraph.name = $"{name}NodeGraph";

            mapNodeGroup.graphType = graphType;
            mapNodeGroup.name = $"{name}MapNodeGroup";


            mapNodeGroup.UpdateNoCeilingMasks();
            mapNodeGroup.UpdateTeleporterMasks();

            mapNodeGroup.Bake(nodeGraph);

            innerWatch.Stop();

            UnityEngine.Debug.Log($"Baked {nodeGraph.GetNodeCount()} {name} nodes in {innerWatch.ElapsedMilliseconds}ms");

            return nodeGraph;
        }

        void SaveGraph<T>(T nodeGraph, string name) where T : ScriptableObject
        {
            var activeScene = SceneManager.GetActiveScene();
            var scenePath = activeScene.path;

            scenePath = System.IO.Path.GetDirectoryName(scenePath);
            nodeGraph.name = name;

            AssetDatabase.CreateAsset(nodeGraph, System.IO.Path.Combine(scenePath, activeScene.name, $"{nodeGraph.name}.asset"));
        }
    }
}
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using UnityEditor.AI;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using GraphType = RoR2.Navigation.MapNodeGroup.GraphType;
using MapNodeGroup = RoR2.Navigation.MapNodeGroup;
using RainOfStages.Configurators;
using SceneInfo = RainOfStages.Proxy.SceneInfo;
using UnityEditor.SceneManagement;

namespace RainOfStages
{
    public class NodeGraphBaker : ScriptableObject
    {
        public BakeSettings bakeSettings;
        [MenuItem("Tools/Rain of Stages/Bake NodeGraph")]
        public static void Bake()
        {
            var baker = ScriptableObject.CreateInstance<NodeGraphBaker>();

            baker.Build();

            DestroyImmediate(baker);
        }

        NavMeshBuildSettings settings => bakeSettings.bakeSettings;

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

            var (triangulation, bounds) = GenerateTriangulation(world.transform);

            GameObject obj = GameObject.Find("NodeMesh");
            if (obj) DestroyImmediate(obj);

            var nodeMesh = new Mesh
            {
                vertices = triangulation.vertices,
                triangles = triangulation.indices
            };
            nodeMesh.RecalculateNormals();
            nodeMesh.RecalculateTangents();
            nodeMesh.RecalculateBounds();

            var nodeGraphMeshObject = bakeSettings.NodeMeshObject = new GameObject("NodeMesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider));
            nodeGraphMeshObject.hideFlags = HideFlags.HideAndDontSave;

            var filter = nodeGraphMeshObject.GetComponent<MeshFilter>();
            filter.sharedMesh = nodeMesh;

            var renderer = nodeGraphMeshObject.GetComponent<MeshRenderer>();
            renderer.material = bakeSettings.DebugMaterial;

            var collider = nodeGraphMeshObject.GetComponent<BoxCollider>();
            collider.size = bounds.size;
            collider.center = bounds.center;

            nodeGraphMeshObject.SetActive(bakeSettings.showMesh);

            if (bakeSettings.DebugMode) return;



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

        (NavMeshTriangulation, Bounds) GenerateTriangulation(Transform world)
        {
            NavMesh.RemoveAllNavMeshData();
            var markups = new List<NavMeshBuildMarkup>();
            var sources = new List<NavMeshBuildSource>();
            UnityEngine.AI.NavMeshBuilder.CollectSources(world, LayerMask.GetMask("World"), NavMeshCollectGeometry.RenderMeshes, 0, markups, sources);
            var renderers = world.gameObject.GetComponentsInChildren<MeshRenderer>().Where(mr => mr.gameObject.layer == LayerMask.NameToLayer("World"));
            var bounds = renderers.Select(r => r.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
            var nvd = UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, world.position, world.rotation);
            NavMesh.AddNavMeshData(nvd);
            var triangulation = NavMesh.CalculateTriangulation();
            NavMesh.RemoveAllNavMeshData();
            return (triangulation, bounds);
        }
        (NavMeshTriangulation, Bounds) GenerateTriangulation2(Transform world)
        {
            NavMesh.RemoveAllNavMeshData();

            var markups = new List<NavMeshBuildMarkup>();
            var sources = new List<NavMeshBuildSource>();
            var renderers = world.gameObject.GetComponentsInChildren<MeshRenderer>();
            var bounds = renderers.Select(r => r.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
            bounds.center = Vector3.zero;
            Scene scene = SceneManager.GetActiveScene();
            UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(world, LayerMask.GetMask("World"), NavMeshCollectGeometry.RenderMeshes, 0, markups, scene, sources);
            //UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject
            var nvd = UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, world.position, world.rotation);
            NavMesh.AddNavMeshData(nvd);
            var triangulation = NavMesh.CalculateTriangulation();

            NavMesh.RemoveAllNavMeshData();
            return (triangulation, bounds);
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

            if (!AssetDatabase.IsValidFolder(System.IO.Path.Combine(scenePath, activeScene.name)))
                AssetDatabase.CreateFolder(scenePath, activeScene.name);

            AssetDatabase.CreateAsset(nodeGraph, System.IO.Path.Combine(scenePath, activeScene.name, $"{nodeGraph.name}.asset"));
        }
    }
}
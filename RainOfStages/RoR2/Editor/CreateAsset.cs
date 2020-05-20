using RainOfStages.Proxy;
using RainOfStages.Proxy.RoR2;
using RainOfStages.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using BodySpawnCard = RainOfStages.Proxy.BodySpawnCard;
using InteractableSpawnCard = RainOfStages.Proxy.InteractableSpawnCard;
using CharacterSpawnCard = RainOfStages.Proxy.CharacterSpawnCard;

namespace RainOfStages.Editor
{
    public class CreateAsset : ScriptableObject
    {
        public GameObject Director;
        public GameObject GlobalEventManager;
        public GameObject SceneInfo;

        [MenuItem("Assets/Rain of Stages/" + nameof(SurfaceDef))]
        public static void CreateSurfaceDef() => ScriptableHelper.CreateAsset<SurfaceDef>();

        [MenuItem("Assets/Rain of Stages/" + nameof(DirectorCardCategorySelection))]
        public static void CreateDirectorCardCategorySelection() => ScriptableHelper.CreateAsset<DirectorCardCategorySelection>();


        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(SpawnCard))]
        public static void CreateSpawnCard() => ScriptableHelper.CreateAsset<SpawnCard>();
        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(InteractableSpawnCard))]
        public static void CreateInteractableSpawnCard() => ScriptableHelper.CreateAsset<InteractableSpawnCard>();
        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(CharacterSpawnCard))]
        public static void CreateCharacterSpawnCard() => ScriptableHelper.CreateAsset<CharacterSpawnCard>();
        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(BodySpawnCard))]
        public static void CreateBodySpawnCard() => ScriptableHelper.CreateAsset<BodySpawnCard>();


        [MenuItem("Assets/Rain of Stages/Stages/" + nameof(SceneDefReference))]
        public static void CreateSceneDefReference() => ScriptableHelper.CreateAsset<SceneDefReference>();

        [MenuItem("Assets/Rain of Stages/Stages/" + nameof(SceneDefinition))]
        public static void CreateCustomSceneProxy() => ScriptableHelper.CreateAsset<SceneDefinition>();

        [MenuItem("Assets/Rain of Stages/Stages/New Stage")]
        public static void CreateStage()
        {
            var createAsset = ScriptableObject.CreateInstance<CreateAsset>();


            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PrefabUtility.InstantiatePrefab(createAsset.Director, scene);
            PrefabUtility.InstantiatePrefab(createAsset.GlobalEventManager, scene);
            PrefabUtility.InstantiatePrefab(createAsset.SceneInfo, scene);
            var worldObject = new GameObject("World");
            worldObject.layer = LayerMask.NameToLayer("World");

            var lightObject = new GameObject("Directional Light (SUN)", typeof(Light));
            lightObject.transform.forward = Vector3.forward + Vector3.down + Vector3.right;

            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            (float h, float s, float v) color = (0, 0, 0);
            Color.RGBToHSV(Color.cyan, out color.h, out color.s, out color.v);
            color.s = 0.5f;
            color.v = 0.8f;
            light.lightmapBakeType = LightmapBakeType.Realtime;
            light.color = Color.HSVToRGB(color.h, color.s, color.v);
            RenderSettings.sun = light;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientSkyColor = light.color;
            RenderSettings.ambientGroundColor = light.color;
            RenderSettings.ambientEquatorColor = light.color;
            RenderSettings.ambientIntensity = 0.8f;
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
            Lightmapping.realtimeGI = false;
            Lightmapping.bakedGI = false;

            DynamicGI.UpdateEnvironment();
            EditorSceneManager.MarkSceneDirty(scene);


            //var tab = new LightingExplorerTab()

        }


        [MenuItem("Assets/Rain of Stages/Modding Assets/" + nameof(BakeSettings))]
        public static void CreateBakeSettings() => ScriptableHelper.CreateAsset<BakeSettings>();


        //[MenuItem("Tools/Rain of Stages/Generate Proxies")]
        public static void GenerateProxies() => ProxyGenerator.GenerateProxies(typeof(RoR2.RoR2Application), "RoR2");

    }
}
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderKit.Addressable.Tools
{

#if UNITY_2020_1_OR_NEWER
    public class AddressablePreviewStage : PreviewSceneStage
    {
        public string StageName;
        protected override GUIContent CreateHeaderContent()
        {
            return new GUIContent(StageName);
        }
    }
#else
    public class AddressablePreviewStage : UnityEditor.SceneView
    {
        private static AddressablePreviewStage instance;
        private Func<SceneHierarchyHooks.SubSceneInfo[]> provideSubScenes;
        public GameObject previewObject;
        private GameObject lightingObj;
        private GameObject previewInstance;

        public static void ShowWindow(GameObject previewObject)
        {
            if (!previewObject)
            {
                Debug.LogError("No Game Objects selected, only GameObjects/Prefabs are supported now");
                return;
            }

            if (!instance)
            {
                // Create the window
                instance = CreateWindow<AddressablePreviewStage>("Preview");
                instance.provideSubScenes = ProvideSubScenes;
                SceneHierarchyHooks.provideSubScenes = instance.provideSubScenes;
            }
            else
            {
                EditorSceneManager.ClosePreviewScene(instance.customScene);
            }

            // Get the object you're selecting in the Unity Editor
            instance.titleContent = instance.GetName();
            instance.previewObject = previewObject;

            // Load a new preview scene
            var scene = EditorSceneManager.NewPreviewScene();

            instance.customScene = scene;

            instance.drawGizmos = false;
            instance.SetupScene();
            instance.Repaint();

            SceneHierarchyHooks.ReloadAllSceneHierarchies();
        }

        private static SceneHierarchyHooks.SubSceneInfo[] ProvideSubScenes()
        {
            return new SceneHierarchyHooks.SubSceneInfo[]
            {
                new SceneHierarchyHooks.SubSceneInfo
                {
                    scene = instance.customScene,
                    sceneName = instance.name,
                    transform = instance.previewObject.transform
                }
            };
        }

        public override void OnEnable()
        {
            base.OnEnable();

            // Set title name
            titleContent = GetName();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        private new void OnDestroy()
        {
            base.OnDestroy();
            if (lightingObj) DestroyImmediate(lightingObj);
            if (previewInstance) DestroyImmediate(previewInstance);
            EditorSceneManager.ClosePreviewScene(instance.customScene);
            SceneHierarchyHooks.provideSubScenes = null;
        }

        void SetupScene()
        {
            if (!lightingObj)// Create lighting
            {
                lightingObj = new GameObject("Lighting");
                lightingObj.transform.eulerAngles = new Vector3(50, -30, 0);
                lightingObj.AddComponent<Light>().type = UnityEngine.LightType.Directional;
                lightingObj.hideFlags = HideFlags.HideAndDontSave | HideFlags.NotEditable;
                SceneManager.MoveGameObjectToScene(lightingObj, customScene);
            }

            if (previewInstance)
                DestroyImmediate(previewInstance);

            previewInstance = Instantiate(previewObject as GameObject);
            previewInstance.transform.position = Vector3.zero;
            SceneManager.MoveGameObjectToScene(previewInstance, customScene);
            Selection.activeObject = previewInstance;
            FrameSelected();
        }

        private GUIContent GetName()
        {
            if (previewObject == null)
                return new GUIContent("NuLL");

            // Setup the title GUI Content (Image, Text, Tooltip options) for the window
            GUIContent titleContent = new GUIContent(previewObject.name);
            if (previewObject is GameObject)
            {
                titleContent.image = EditorGUIUtility.IconContent("GameObject Icon").image;
            }
            else if (previewObject is SceneAsset)
            {
                titleContent.image = EditorGUIUtility.IconContent("SceneAsset Icon").image;
            }

            return titleContent;
        }

        new void OnGUI()
        {
            base.OnGUI();
        }
    }
#endif
}
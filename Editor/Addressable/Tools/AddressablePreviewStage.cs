#if TK_ADDRESSABLE
#if UNITY_2020_1_OR_NEWER
#else
using System;
using System.Collections.Generic;
using System.Linq;
#endif
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderKit.Addressable.Tools
{
    public class AddressablePreviewStage :
#if UNITY_2020_1_OR_NEWER
        PreviewSceneStage
#else
        UnityEditor.SceneView
#endif
    {
        private GameObject previewInstance;
        private static AddressablePreviewStage previewStage;

#if UNITY_2020_1_OR_NEWER
        public string StageName;
        protected override GUIContent CreateHeaderContent()
        {
            return new GUIContent(StageName);
        }
#else
        private readonly List<int> selection = new List<int>();
        private TransformHierarchyTreeView transformHierarchyTreeView;
        private Vector2 scrollPosition;
        private Rect viewRect;
        private Scene scene { get => customScene; set => customScene = value; }
#endif
        public static void Show(GameObject previewObject)
        {
            if (!previewObject)
            {
                Debug.LogError("No Game Objects selected, only GameObjects/Prefabs are supported now");
                return;
            }

#if UNITY_2020_1_OR_NEWER
            previewStage = CreateInstance<AddressablePreviewStage>();
            StageUtility.GoToStage(previewStage, true);
#else
            previewStage = GetWindow<AddressablePreviewStage>("Preview");
            SceneHierarchyHooks.provideSubScenes = previewStage.ProvideSubScenes;
            if (!previewStage.scene.IsValid())
                previewStage.scene = EditorSceneManager.NewPreviewScene();

            SceneView.duringSceneGui += previewStage.SceneView_duringSceneGui;
#endif
            previewStage.SetupScene(previewObject);

#if UNITY_2020_1_OR_NEWER
            SceneView.lastActiveSceneView.FrameSelected();
            SceneView.lastActiveSceneView.Repaint();
#else
            previewStage.FrameSelected();
            previewStage.Repaint();
            SceneHierarchyHooks.ReloadAllSceneHierarchies();
#endif
        }

        GameObject CreatePreviewInstance(GameObject previewObject)
        {
            var instance = Instantiate(previewObject);

            SetRecursiveFlags(instance.transform);
            SceneManager.MoveGameObjectToScene(instance, scene);
            return instance;
        }

        void SetRecursiveFlags(Transform transform)
        {
            transform.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            for (int i = 0; i < transform.childCount; i++)
                SetRecursiveFlags(transform.GetChild(i));
        }

        void SetupScene(GameObject previewObject)
        {
            if (previewInstance)
                DestroyImmediate(previewInstance);

            previewInstance = CreatePreviewInstance(previewObject);
            name = "Preview";
            var scene = this.scene;
            scene.name = previewObject.name;
            this.scene = scene;
            this.sceneLighting = false;

#if UNITY_2020_1_OR_NEWER
            StageName = previewInstance.name;
#else
            transformHierarchyTreeView = new TransformHierarchyTreeView(new UnityEditor.IMGUI.Controls.TreeViewState(), previewInstance.transform);
            transformHierarchyTreeView.Reload();
            titleContent = new GUIContent(name)
            {
                image = EditorGUIUtility.IconContent("GameObject Icon").image
            };
#endif
            //ThunderStageUtility.InstantiateStageLight(scene, 45);
            //ThunderStageUtility.InstantiateStageLight(scene, -45, 180);
            //ThunderStageUtility.InstantiateStageLight(scene, -45, 90);
            //ThunderStageUtility.InstantiateStageLight(scene, 45, -90);

            previewInstance.transform.position = Vector3.zero;
            Selection.activeObject = previewInstance;
#if UNITY_2020_1_OR_NEWER
#else
            FrameSelected();
#endif
        }
        private void OnDestroy()
        {
            Cleanup();
#if UNITY_2020_1_OR_NEWER
            base.OnCloseStage();
#else
            base.OnDestroy();
#endif
        }
        private void Cleanup()
        {
            if (previewInstance) DestroyImmediate(previewInstance);
#if UNITY_2020_1_OR_NEWER
#else
            if (previewStage)
                EditorSceneManager.ClosePreviewScene(previewStage.scene);
            SceneHierarchyHooks.provideSubScenes = null;
            previewStage = null;
#endif
        }
#if UNITY_2020_1_OR_NEWER
#else
        SceneHierarchyHooks.SubSceneInfo[] ProvideSubScenes()
        {
            if (!previewInstance) return Array.Empty<SceneHierarchyHooks.SubSceneInfo>();

            return new SceneHierarchyHooks.SubSceneInfo[]
            {
                new SceneHierarchyHooks.SubSceneInfo
                {
                    scene = scene,
                    sceneName = previewInstance.name,
                    transform = previewInstance.transform
                }
            };
        }
        public override void OnDisable()
        {
            base.OnDisable();
            Cleanup();
        }
        new void OnGUI()
        {
            base.OnGUI();
        }

        int lastSelection = 0;
        private void SceneView_duringSceneGui(SceneView obj)
        {
            if (obj != this) return;
            var position = SceneView.lastActiveSceneView.position;
            var guiPosition = GUIUtility.ScreenToGUIRect(position);
            float height = guiPosition.height - 21;
            viewRect = new Rect(0, 0, 300, height);

            scrollPosition = GUI.BeginScrollView(new Rect(0, 0, 300, height), scrollPosition, viewRect);

            selection.Clear();
            if (Selection.activeGameObject)
            {
                var transform = Selection.activeGameObject.transform;
                if (transform && lastSelection != transform.GetInstanceID())
                {
                    lastSelection = transform.GetInstanceID();
                    selection.Add(transform.GetInstanceID());
                    transformHierarchyTreeView.SetSelection(selection);
                    transformHierarchyTreeView.FrameItem(transform.GetInstanceID());
                }
            }

            transformHierarchyTreeView.OnGUI(new Rect(0, 0, 300, height));

            GUI.EndScrollView();

            switch (Event.current.keyCode)
            {
                case KeyCode.F:
                    obj.FrameSelected();
                    break;
            }
        }

#endif



    }
}
#endif
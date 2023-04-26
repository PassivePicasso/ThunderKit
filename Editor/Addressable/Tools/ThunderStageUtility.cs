using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ThunderKit.Addressable.Tools
{
    public static class ThunderStageUtility
    {
        private static MethodInfo openPreviewScene;

        static ThunderStageUtility()
        {
            openPreviewScene = typeof(EditorSceneManager).GetMethod("OpenPreviewScene", new[] { typeof(string) });
        }


        internal static Scene LoadOrCreatePreviewScene(string environmentEditingScenePath)
        {
            Scene previewScene;
            if (!string.IsNullOrEmpty(environmentEditingScenePath))
                previewScene = (Scene)openPreviewScene.Invoke(null, new[] { environmentEditingScenePath });
            else
                previewScene = CreateDefaultPreviewScene();

            return previewScene;
        }

        internal static Scene CreateDefaultPreviewScene()
        {
            Scene previewScene = EditorSceneManager.NewPreviewScene();

            // Setup default render settings for this preview scene
            Unsupported.SetOverrideLightingSettings(previewScene);
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat") as Material;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            Unsupported.RestoreOverrideLightingSettings();

            return previewScene;
        }


        internal static void InstantiateStageLight(Scene scene, float x = 0, float y = 0, float z = 0)
        {
            var stageLight = new GameObject("Stage Lighting", typeof(Light));
            var light = stageLight.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.5f;
            var stageLightTransform = stageLight.GetComponent<Transform>();
            stageLightTransform.rotation = Quaternion.Euler(x, y, z);
            stageLight.hideFlags = HideFlags.HideAndDontSave;
            SceneManager.MoveGameObjectToScene(stageLight, scene);
        }
    }
}
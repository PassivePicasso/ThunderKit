#if TK_ADDRESSABLE
using System.IO;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UIElements;

namespace ThunderKit.RemoteAddressables
{
    public class AddressableGraphicsSettings : ThunderKitSetting
    {
        public string CustomDeferredReflection;
        public string CustomDeferredShading;
        public string CustomDeferredScreenspaceShadows;

        [InitializeOnLoadMethod]
        public static void OnLoad()
        {
            Addressables.InternalIdTransformFunc = RedirectInternalIdsToGameDirectory;
            SetAllShaders();
            CompilationPipeline.compilationStarted -= ClearSlectionIfUnsavable;
            CompilationPipeline.compilationStarted += ClearSlectionIfUnsavable;
        }

        private static void ClearSlectionIfUnsavable(object obj)
        {
            if (!Selection.activeObject) return;
            if (Selection.activeObject.hideFlags.HasFlag(HideFlags.DontSave))
                Selection.activeObject = null;
        }

        public static void SetAllShaders()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            Addressables.InitializeAsync().WaitForCompletion();

            var settings = GetOrCreateSettings<AddressableGraphicsSettings>();
            SetShader(settings.CustomDeferredShading, BuiltinShaderType.DeferredShading);
            SetShader(settings.CustomDeferredReflection, BuiltinShaderType.DeferredReflections);
            SetShader(settings.CustomDeferredScreenspaceShadows, BuiltinShaderType.ScreenSpaceShadows);
        }

        public static void UnsetAllShaders()
        {
            var settings = GetOrCreateSettings<AddressableGraphicsSettings>();
            UnsetShader(settings.CustomDeferredShading, BuiltinShaderType.DeferredShading);
            UnsetShader(settings.CustomDeferredReflection, BuiltinShaderType.DeferredReflections);
            UnsetShader(settings.CustomDeferredScreenspaceShadows, BuiltinShaderType.ScreenSpaceShadows);
        }

        static string RedirectInternalIdsToGameDirectory(IResourceLocation location)
        {
            var path = location.InternalId.Replace("\\", "/");

            var standardPwd = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "StreamingAssets", "aa").Replace("\\", "/");
            if (location.ResourceType == typeof(IAssetBundleResource) && path.StartsWith(standardPwd))
                path = path.Replace(standardPwd, ThunderKitSettings.EditTimePath);

            return path;
        }

        public static void SetShader(string address, BuiltinShaderType shaderType)
        {
            if (!string.IsNullOrEmpty(address))
            {
                var cdr = Addressables.LoadAssetAsync<Shader>(address).WaitForCompletion();
                cdr.hideFlags = HideFlags.HideAndDontSave;
                GraphicsSettings.SetCustomShader(shaderType, cdr);
                GraphicsSettings.SetShaderMode(shaderType, BuiltinShaderMode.UseCustom);
            }
            else GraphicsSettings.SetShaderMode(shaderType, BuiltinShaderMode.UseBuiltin);
        }

        public static void UnsetShader(string address, BuiltinShaderType shaderType)
        {
            if (!string.IsNullOrEmpty(address))
            {
                GraphicsSettings.SetCustomShader(shaderType, null);
                GraphicsSettings.SetShaderMode(shaderType, BuiltinShaderMode.UseBuiltin);
            }
        }

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            base.CreateSettingsUI(rootElement);
            rootElement.Add(new Button(() => SetAllShaders()) { text = "Reload" });
        }
    }
}
#endif

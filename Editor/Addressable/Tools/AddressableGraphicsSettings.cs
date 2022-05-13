#if TK_ADDRESSABLE
using System;
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

namespace ThunderKit.Addressable.Tools
{
    public class AddressableGraphicsSettings : ThunderKitSetting
    {
        public string CustomDeferredReflection;
        public string CustomDeferredShading;
        public string CustomDeferredScreenspaceShadows;

        public static event EventHandler AddressablesInitialized;

        [InitializeOnLoadMethod]
        public static void OnLoad()
        {
            Addressables.InternalIdTransformFunc = RedirectInternalIdsToGameDirectory;
            CompilationPipeline.compilationStarted -= ClearSelectionIfUnsavable;
            CompilationPipeline.compilationStarted += ClearSelectionIfUnsavable;
            InitializeAddressables();
        }

        static string RedirectInternalIdsToGameDirectory(IResourceLocation location)
        {
            switch (location.ResourceType)
            {
                case var t when t == typeof(IAssetBundleResource):
                    var iid = location.InternalId;
                    var path = iid.Substring(iid.IndexOf("/aa") + 4);
                    path = Path.Combine(ThunderKitSettings.EditTimePath, path);
                    return path;
                default:
                    var result = location.InternalId;
                    return result;
            }
        }

        static void InitializeAddressables()
        {
            var aop = Addressables.InitializeAsync();
            aop.WaitForCompletion();
            AssignShaders();
        }

        private static void ClearSelectionIfUnsavable(object obj)
        {
            if (!Selection.activeObject) return;
            if (Selection.activeObject.hideFlags.HasFlag(HideFlags.DontSave))
                Selection.activeObject = null;
        }

        private static void AssignShaders()
        {
            var settings = GetOrCreateSettings<AddressableGraphicsSettings>();
            SetShader(settings.CustomDeferredShading, BuiltinShaderType.DeferredShading);
            SetShader(settings.CustomDeferredReflection, BuiltinShaderType.DeferredReflections);
            SetShader(settings.CustomDeferredScreenspaceShadows, BuiltinShaderType.ScreenSpaceShadows);
            AddressablesInitialized?.Invoke(null, EventArgs.Empty);
        }

        public static void UnsetAllShaders()
        {
            var settings = GetOrCreateSettings<AddressableGraphicsSettings>();
            UnsetShader(settings.CustomDeferredShading, BuiltinShaderType.DeferredShading);
            UnsetShader(settings.CustomDeferredReflection, BuiltinShaderType.DeferredReflections);
            UnsetShader(settings.CustomDeferredScreenspaceShadows, BuiltinShaderType.ScreenSpaceShadows);
        }
        public static void SetShader(string address, BuiltinShaderType shaderType)
        {
            if (!string.IsNullOrEmpty(address))
            {
                var aop = Addressables.LoadAssetAsync<Shader>(address);
                aop.WaitForCompletion();
                if (aop.Result is Shader shader && shader)
                {
                    shader.hideFlags = HideFlags.HideAndDontSave;
                    GraphicsSettings.SetCustomShader(shaderType, shader);
                    GraphicsSettings.SetShaderMode(shaderType, BuiltinShaderMode.UseCustom);
                }
                else
                    Debug.LogError($"Custom {shaderType} shader at address \"{address}\" can't be assigned");
            }
            else
                GraphicsSettings.SetShaderMode(shaderType, BuiltinShaderMode.UseBuiltin);
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
            rootElement.Add(new Button(InitializeAddressables) { text = "Reload" });
        }
    }
}
#endif

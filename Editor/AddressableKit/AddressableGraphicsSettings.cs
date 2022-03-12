#if TK_ADDRESSABLE
using System.IO;
using System.Threading.Tasks;
using ThunderKit.Core.Data;
using UnityEditor;
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [InitializeOnLoadMethod]
        public static async Task SetAllShaders()
        {
            Addressables.InternalIdTransformFunc = RedirectInternalIdsToGameDirectory;
            AssetBundle.UnloadAllAssetBundles(true);
            var initializeOpertaion = Addressables.InitializeAsync();
            bool wait = true;
            while (wait && !initializeOpertaion.IsDone)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                await Task.Delay(500);
            }
            var settings = GetOrCreateSettings<AddressableGraphicsSettings>();
            await SetShader(settings.CustomDeferredShading, BuiltinShaderType.DeferredShading);
            await SetShader(settings.CustomDeferredReflection, BuiltinShaderType.DeferredReflections);
            await SetShader(settings.CustomDeferredScreenspaceShadows, BuiltinShaderType.ScreenSpaceShadows);
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

        public static async Task SetShader(string address, BuiltinShaderType shaderType)
        {
            if (!string.IsNullOrEmpty(address))
            {
                var cdrOp = Addressables.LoadAssetAsync<Shader>(address);
                bool wait = true;
                while (wait && !cdrOp.IsDone)
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                    await Task.Delay(500);
                }
                var cdr = cdrOp.Result;
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
            rootElement.Add(new Button(async () => await SetAllShaders()) { text = "Reload" });
        }
    }
}
#endif

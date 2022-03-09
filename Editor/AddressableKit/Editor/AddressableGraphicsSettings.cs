#if TK_ADDRESSABLE
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

namespace ThunderKit.RemoteAddressables
{
    public class AddressableGraphicsSettings
    {
        [InitializeOnLoadMethod]
        static async void Start()
        {
            var shader = await Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/Internal-DeferredReflectionsCustom.shader").Task;
            shader.hideFlags = HideFlags.HideAndDontSave;
            GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredReflections, shader);

            shader = await Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/Internal-DeferredShadingCustom.shader").Task;
            shader.hideFlags = HideFlags.HideAndDontSave;
            GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredShading, shader);
            GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, BuiltinShaderMode.UseCustom);
            GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredReflections, BuiltinShaderMode.UseCustom);
            GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseCustom);
        }
    }
}
#endif
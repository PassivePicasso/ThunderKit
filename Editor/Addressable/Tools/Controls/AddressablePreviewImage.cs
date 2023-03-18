using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Addressable.Tools
{
    public class AddressablePreviewImage : Image
    {
        private const string Library = "Library";
        private const string SimplyAddress = "SimplyAddress";
        private const string Previews = "Previews";

        private static readonly Dictionary<string, Texture2D> PreviewCache = new Dictionary<string, Texture2D>();
        private static string PreviewRoot => Path.Combine(Library, SimplyAddress, Previews);
        private static Texture sceneIcon;

        private Texture2D texture;

        public new class UxmlFactory : UxmlFactory<AddressablePreviewImage, UxmlTraits> { }
        public new class UxmlTraits : Image.UxmlTraits { }

        static AddressablePreviewImage()
        {
            sceneIcon = EditorGUIUtility.IconContent("d_UnityLogo").image;
        }

        public async Task Render(IResourceLocation location, string address, bool isAsset)
        {
            image = null;
            if (isAsset)
            {
                texture = await RenderIcon(address);
                if (texture) image = texture;
            }
            else if (location.ResourceType == typeof(SceneInstance))
                image = sceneIcon;
        }

        private async Task<Texture2D> RenderIcon(string address)
        {
            string previewCachePath = Path.Combine(PreviewRoot, $"{address}.png");
            if (File.Exists(previewCachePath))
            {
                var texture = new Texture2D(128, 128);
                texture.LoadImage(File.ReadAllBytes(previewCachePath));
                texture.Apply();
                PreviewCache[address] = texture;
                return texture;
            }

            Texture2D preview = null;
            Object result = null;
            try
            {
                result = await Addressables.LoadAssetAsync<Object>(address).Task;
                preview = UpdatePreview(result);
            }
            catch
            {
            }
            if (result)
                while (AssetPreview.IsLoadingAssetPreviews())
                {
                    await Task.Delay(100);
                    preview = UpdatePreview(result);
                    if (preview && preview.isReadable)
                    {
                        var png = preview.EncodeToPNG();
                        var fileName = $"{Path.GetFileName(address)}.png";
                        string addressFolder = Path.GetDirectoryName(address);
                        var finalFolder = Path.Combine(PreviewRoot, addressFolder);
                        Directory.CreateDirectory(finalFolder);
                        var filePath = Path.Combine(finalFolder, fileName);
                        File.WriteAllBytes(filePath, png);
                    }
                }

            return preview;
        }
        private Texture2D UpdatePreview(Object result)
        {
            Texture2D preview;
            switch (result)
            {
                case GameObject gobj when gobj.GetComponentsInChildren<SkinnedMeshRenderer>().Any()
                                       || gobj.GetComponentsInChildren<SpriteRenderer>().Any()
                                       || gobj.GetComponentsInChildren<MeshRenderer>().Any()
                                       || gobj.GetComponentsInChildren<CanvasRenderer>().Any():
                case Material _:
                    preview = AssetPreview.GetAssetPreview(result);
                    break;
                default:
                    preview = AssetPreview.GetMiniThumbnail(result);
                    break;
            }

            return preview;
        }
    }
}
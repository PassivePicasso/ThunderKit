using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.Helpers
{
    using static Helpers.UnityPathUtility;
    public static class ImageElementFactory
    {
        [Serializable]
        public struct ImageCacheRecord
        {
            public string Url;
            public string Hash;
        }
        [Serializable]
        public struct ImageCache
        {
            public long Size;
            public ImageCacheRecord[] Records;
        }

        private static long size;
        private static int count;
        private static Dictionary<string, string> CacheRecords = new Dictionary<string, string>();
        private static GameObject imageLoaderObject;
        private static ImageLoadBehaviour imageLoader;

        public static long Size => size;
        public static int Count => count;
        public static string CachePath = "Library/MarkdownImageCache";
        private static string CacheRecordsPath => Path.Combine(GetCacheRoot(), "cacheRecords.json");

        private static string GetCacheRoot() => Path.Combine(Directory.GetCurrentDirectory(), CachePath.TrimStart('/'));

        public static event EventHandler CacheUpdated;

        [InitializeOnLoadMethod]
        static void BeforeUnload()
        {
            LoadCacheRecords();
            AssemblyReloadEvents.beforeAssemblyReload += SaveCacheRecords;
        }

        private static bool IsCachedImage(string url) => CacheRecords.ContainsKey(url);
        private static void LoadCacheRecords()
        {
            if (File.Exists(CacheRecordsPath))
            {
                var cacheRecordsJson = File.ReadAllText(CacheRecordsPath);
                var cacheRecords = JsonUtility.FromJson<ImageCache>(cacheRecordsJson);
                foreach (var record in cacheRecords.Records)
                    CacheRecords[record.Url] = record.Hash;

                size = cacheRecords.Size;
                count = cacheRecords.Records.Length;
            }
        }
        private static void SaveCacheRecords()
        {
            var cacheRecords = CacheRecords.Select(kvp => new ImageCacheRecord { Url = kvp.Key, Hash = kvp.Value }).ToArray();
            var cacheRecordsJson = JsonUtility.ToJson(new ImageCache { Records = cacheRecords, Size = size }, true);
            try
            {
                File.Delete(CacheRecordsPath);
            }
            catch {/* Do nothing because we don't care if the delete fails we are overwriting it anyawys*/}
            File.WriteAllText(CacheRecordsPath, cacheRecordsJson);
        }
        public static void ClearCache()
        {
            CacheRecords.Clear();
            foreach (var file in Directory.EnumerateFiles(GetCacheRoot()))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {

                }
            }
            size = 0;
            count = 0;
            SaveCacheRecords();
            CacheUpdated?.Invoke(null, EventArgs.Empty);
        }


        internal class ImageLoadBehaviour : MonoBehaviour { }
        public static Image GetImageElement(string url, params string[] classNames)
        {
            var imageElement = VisualElementFactory.GetClassedElement<Image>(classNames);
            if (IsAssetDirectory(url))
            {
                var image = AssetDatabase.LoadAssetAtPath<Texture>(url);
                if (image)
                    SetupImage(imageElement, image);
            }
            else if (IsCachedImage(url))
            {
                var imageHash = CacheRecords[url];
                var imagePath = Path.GetFullPath(Path.Combine(GetCacheRoot(), $"{imageHash}.png"));
                var imageBytes = File.ReadAllBytes(imagePath);
                var image = new Texture2D(1, 1);
                if (ImageConversion.LoadImage(image, imageBytes))
                    SetupImage(imageElement, image);
            }
            else
            {
                if (!imageLoaderObject || !imageLoader)
                {
                    imageLoaderObject = new GameObject("MarkdownImageLoader", typeof(ImageLoadBehaviour)) { isStatic = true, hideFlags = HideFlags.HideAndDontSave };
                    imageLoader = imageLoaderObject.GetComponent<ImageLoadBehaviour>();
                }
                var c = imageLoader.StartCoroutine(LoadImage(url, imageElement, imageLoaderObject));
            }
            return imageElement;
        }


        static IEnumerator LoadImage(string url, Image imageElement, GameObject gameObject)
        {
            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
                if (request.isNetworkError || request.isHttpError)
#endif
                {
                    if (!request.error.Contains("aborted"))
                        Debug.Log(request.error);
                }
                else
                {
                    var downloadHandler = (DownloadHandlerTexture)request.downloadHandler;
                    var texture = downloadHandler.texture;

                    SetupImage(imageElement, texture);
                    try
                    {
                        var imageHash = $"{texture.imageContentsHash}";
                        var cacheRoot = GetCacheRoot();
                        var fullPath = Path.GetFullPath(Path.Combine(cacheRoot, $"{imageHash}.png"));
                        if (!File.Exists(fullPath))
                        {
                            var pngBytes = texture.EncodeToPNG();
                            size += pngBytes.Length;
                            count++;
                            Directory.CreateDirectory(cacheRoot);
                            File.WriteAllBytes(fullPath, pngBytes);
                        }
                        var containedKey = CacheRecords.ContainsKey(url);
                        CacheRecords[url] = imageHash;
                        if (!containedKey)
                        {
                            SaveCacheRecords();
                        }
                        CacheUpdated?.Invoke(null, EventArgs.Empty);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }
        }


        private static void DestroyTexture(DetachFromPanelEvent evt)
        {
            var imageElement = evt.target as Image;
#if UNITY_2019_1_OR_NEWER
            var texture = imageElement.image;
#else
            var texture = imageElement.image.value;
#endif
            if (texture)
                UnityEngine.Object.DestroyImmediate(texture);
        }

        private static void SetupImage(Image imageElement, Texture texture)
        {
            imageElement.image = texture;
            imageElement.scaleMode = ScaleMode.ScaleAndCrop;
            imageElement.RegisterCallback<DetachFromPanelEvent>(DestroyTexture);
        }
    }
}
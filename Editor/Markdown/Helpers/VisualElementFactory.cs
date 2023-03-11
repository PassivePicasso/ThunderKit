using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.Helpers
{
    using static Helpers.UnityPathUtility;
    public static class VisualElementFactory
    {
        private struct ImageCacheRecord
        {
            public string Url;
            public string Hash;
        }

        private static Dictionary<string, string> CacheRecords = new Dictionary<string, string>();
        private static GameObject imageLoaderObject;
        private static ImageLoadBehaviour imageLoader;

        public static string CachePath = "Library/MarkdownImageCache";
        private static string CacheRecordsPath => Path.Combine(GetCacheRoot(), "cacheRecords.json");

        private static string GetCacheRoot() => Path.Combine(Directory.GetCurrentDirectory(), CachePath.TrimStart('/'));

        [InitializeOnLoadMethod]
        static void BeforeUnload()
        {
            AssemblyReloadEvents.beforeAssemblyReload += SaveCacheRecords;
        }

        private static void SaveCacheRecords()
        {
            var cacheRecords = CacheRecords.Select(kvp => new ImageCacheRecord { Url = kvp.Key, Hash = kvp.Value }).ToList();
            var cacheRecordsJson = JsonUtility.ToJson(cacheRecords);
            File.Delete(CacheRecordsPath);
            File.WriteAllText(CacheRecordsPath, cacheRecordsJson);
        }

        internal class ImageLoadBehaviour : MonoBehaviour { }
        public static T GetImageElement<T>(string url, params string[] classNames) where T : Image, new()
        {
            T imageElement = GetClassedElement<T>(classNames);
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

        private static bool IsCachedImage(string url)
        {
            if (CacheRecords.Count == 0)
            {
                if (File.Exists(CacheRecordsPath))
                {
                    var cacheRecordsJson = File.ReadAllText(CacheRecordsPath);
                    var cacheRecords = JsonUtility.FromJson<List<ImageCacheRecord>>(cacheRecordsJson);
                    foreach (var record in cacheRecords)
                        CacheRecords[record.Url] = record.Hash;
                }
            }
            if (CacheRecords.ContainsKey(url)) return true;

            return false;
        }

        static IEnumerator LoadImage(string url, Image imageElement, GameObject gameObject)
        {
            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                imageElement.RegisterCallback<DetachFromPanelEvent, UnityWebRequest>(CancelRequest, request);

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
                        var pngBytes = texture.EncodeToPNG();
                        var imageHash = $"{texture.imageContentsHash}";
                        var fullPath = Path.GetFullPath(Path.Combine(GetCacheRoot(), $"{imageHash}.png"));
                        if (!File.Exists(fullPath))
                            File.WriteAllBytes(fullPath, pngBytes);
                        CacheRecords[url] = imageHash;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }

                imageElement.UnregisterCallback<DetachFromPanelEvent, UnityWebRequest>(CancelRequest);
            }
        }


        private static void DestroyTexture(DetachFromPanelEvent evt)
        {
            var imageElement = evt.target as Image;
            var texture = imageElement.image;
            if (texture)
                UnityEngine.Object.DestroyImmediate(texture);
        }

        static void CancelRequest(DetachFromPanelEvent evt, UnityWebRequest webRequest) => webRequest.Abort();

        static void SetupImage(Image imageElement, Texture texture)
        {
            imageElement.image = texture;
            imageElement.scaleMode = ScaleMode.ScaleAndCrop;
            imageElement.RegisterCallback<DetachFromPanelEvent>(DestroyTexture);
        }

        public static T GetTextElement<T>(string text, string className) where T : TextElement, new()
        {
            T element = GetClassedElement<T>(className);

            element.text = text;
            element.pickingMode = PickingMode.Ignore;

            return element;
        }

        public static T GetTextElement<T>(string text, params string[] classNames) where T : TextElement, new()
        {
            T element = GetClassedElement<T>(classNames);

            element.text = text;
            element.pickingMode = PickingMode.Ignore;

            return element;
        }

        public static T GetClassedElement<T>(string className) where T : VisualElement, new()
        {
            T element = new T
            {
                name = className
            };
            element.AddToClassList(className);

            return element;
        }

        public static T GetClassedElement<T>(params string[] classNames) where T : VisualElement, new()
        {
            T element = new T();

            if (classNames == null || classNames.Length == 0) return element;
            element.name = classNames[0];

            for (int i = 0; i < classNames.Length; i++)
                element.AddToClassList(classNames[i]);

            return element;
        }
    }
}
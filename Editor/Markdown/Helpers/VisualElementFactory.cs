using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using ThunderKit.Markdown.ObjectRenderers;
using UnityEditor;
using System;
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
        private static GameObject imageLoaderObject;
        private static ImageLoadBehaviour imageLoader;

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
                    imageElement.RegisterCallback<DetachFromPanelEvent>(DestroyTexture);
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
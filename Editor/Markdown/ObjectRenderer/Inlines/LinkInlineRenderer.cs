using System;
using Markdig.Syntax.Inlines;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using System.Reflection;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif
using Object = UnityEngine.Object;

namespace ThunderKit.Markdown.ObjectRenderers
{

    using static Helpers.VisualElementFactory;
    using static Helpers.UnityPathUtility;
    public class LinkInlineRenderer : UIElementObjectRenderer<LinkInline>
    {
        private const BindingFlags nonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;
        public struct SchemeHandler
        {
            public Action<string> linkHandler;
            public Func<Label, VisualElement> preprocessor;
        }
        internal class ImageLoadBehaviour : MonoBehaviour { }

        internal static Regex SchemeCheck = new Regex("^([\\w]+)://.*", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Func<Object, Texture2D> GetIconForObject;
        internal static Dictionary<string, SchemeHandler> SchemeLinkHandlers;

        [InitializeOnLoadMethod]
        static void InitializeDefaultSchemes()
        {
            RegisterScheme("assetlink", link =>
            {
                var schemelessUri = link.Substring("assetlink://".Length);
                if (schemelessUri.Length == 0) return;
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(schemelessUri);
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            },
            label =>
            {
                var schemelessUri = label.tooltip.Substring("assetlink://".Length);
                if (schemelessUri.Length == 0)
                    return label;

                var container = new VisualElement();

                var icon = new Image();
                icon.AddToClassList("asset-icon");
                icon.image = AssetDatabase.GetCachedIcon(schemelessUri);

                container.Add(icon);
                container.Add(label);

                return container;
            });
            RegisterScheme("menulink", link =>
            {
                var schemelessUri = link.Substring("menulink://".Length);
                if (schemelessUri.Length == 0) return;
                EditorApplication.ExecuteMenuItem(schemelessUri);
            });
            RegisterScheme("http", link => System.Diagnostics.Process.Start(link));
            RegisterScheme("https", link => System.Diagnostics.Process.Start(link));
            RegisterScheme("mailto", link => System.Diagnostics.Process.Start(link));
        }

        public static bool RegisterScheme(string scheme, Action<string> action, Func<Label, VisualElement> preprocessor = null)
        {
            if (SchemeLinkHandlers == null) SchemeLinkHandlers = new Dictionary<string, SchemeHandler>();

            if (!SchemeLinkHandlers.ContainsKey(scheme))
            {
                SchemeLinkHandlers[scheme] = new SchemeHandler
                {
                    linkHandler = action,
                    preprocessor = preprocessor != null ? preprocessor : label => label
                };
                return true;
            }
            return false;
        }

        IEnumerator LoadImage(string url, Image imageElement)
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
                    Debug.Log(request.error);
                else
                    SetupImage(imageElement, ((DownloadHandlerTexture)request.downloadHandler).texture);

                imageElement.UnregisterCallback<DetachFromPanelEvent, UnityWebRequest>(CancelRequest);
            }
        }

        static void CancelRequest(DetachFromPanelEvent evt, UnityWebRequest webRequest) => webRequest.Abort();

        public static void SetupImage(Image imageElement, Texture texture)
        {
            imageElement.image = texture;
#if UNITY_2019_1_OR_NEWER
            imageElement.style.width = texture.width;
            imageElement.style.height = texture.height;
#else
            imageElement.style.width = new StyleValue<float>(texture.width);
            imageElement.style.height = new StyleValue<float>(texture.height);
#endif
        }

        protected override void Write(UIElementRenderer renderer, LinkInline link)
        {
            var url = UnityWebRequest.UnEscapeURL(link.Url);
            if (link.IsImage)
            {
                var imageElement = GetClassedElement<Image>("image");
                if (IsAssetDirectory(url))
                {
                    var image = AssetDatabase.LoadAssetAtPath<Texture>(url);
                    if (image)
                        SetupImage(imageElement, image);
                }
                else
                {
                    var imageLoaderObject = new GameObject("MarkdownImageLoader", typeof(ImageLoadBehaviour)) { isStatic = true, hideFlags = HideFlags.HideAndDontSave };
                    var imageLoader = imageLoaderObject.GetComponent<ImageLoadBehaviour>();
                    var c = imageLoader.StartCoroutine(LoadImage(url, imageElement));
                }

                renderer.Push(imageElement);
                renderer.WriteChildren(link);
                foreach (var child in imageElement.Children())
                    child.AddToClassList("alt-text");
                renderer.Pop();
            }
            else
            {

                var lowerScheme = string.Empty;
                var match = LinkInlineRenderer.SchemeCheck.Match(url);
                if (match.Success) lowerScheme = match.Groups[1].Value.ToLower();
                else lowerScheme = "#";

                var linkLabel = GetClassedElement<Label>("link", lowerScheme, "inline");
                linkLabel.text = link.FirstChild.ToString();
                linkLabel.userData = url;
                linkLabel.tooltip = url;
                VisualElement inlineElement = linkLabel;
                if (SchemeLinkHandlers.TryGetValue(lowerScheme, out var schemeHandlers))
                {
                    inlineElement = schemeHandlers.preprocessor(linkLabel);

                    if (match.Success)
                        linkLabel.RegisterCallback<MouseUpEvent>(evt => schemeHandlers.linkHandler?.Invoke(url));
                }
                renderer.WriteElement(inlineElement);
            }

        }

    }
}

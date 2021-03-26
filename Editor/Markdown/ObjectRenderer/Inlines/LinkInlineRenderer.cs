using System;
using Markdig.Syntax.Inlines;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.ObjectRenderers
{
    using static Helpers.VisualElementFactory;
    using static Helpers.VisualElementUtility;
    using static Helpers.UnityPathUtility;
    public class LinkInlineRenderer : UIElementObjectRenderer<LinkInline>
    {
        internal static Regex SchemeCheck = new Regex("^([\\w]+)://.*", RegexOptions.Singleline | RegexOptions.Compiled);
        internal static readonly Dictionary<string, Action<string>> SchemeLinkHandlers = new Dictionary<string, Action<string>>
        {
            { "http",  link => System.Diagnostics.Process.Start(link) },
            { "https",  link => System.Diagnostics.Process.Start(link) },
            { "mailto",  link => System.Diagnostics.Process.Start(link) },
            {
                "assetlink",
                link =>
                {
                    var schemelessUri = link.Substring("assetlink://".Length);
                    if(schemelessUri.Length == 0) return;
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(schemelessUri);
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                }
            },
            {
                "menulink",
                link =>
                {
                    var schemelessUri = link.Substring("menulink://".Length);
                    if(schemelessUri.Length == 0) return;
                    EditorApplication.ExecuteMenuItem(schemelessUri);
                }
            },
            {
                "pathreference",
                link =>
                {

                }
            },
            {
                "#",
                link =>{}
            }
        };


        protected override void Write(UIElementRenderer renderer, LinkInline link)
        {
            var url = link.Url;
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
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(link.Url);
                    var asyncOp = request.SendWebRequest();
                    asyncOp.completed += (obj) =>
                    {
#if UNITY_2020_1_OR_NEWER
                        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
                        if (request.isNetworkError || request.isHttpError)
#endif
                            Debug.Log(request.error);
                        else
                            SetupImage(imageElement, ((DownloadHandlerTexture)request.downloadHandler).texture);
                    };
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
                var isValidUri = Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
                if (!isValidUri)
                {
                    var match = LinkInlineRenderer.SchemeCheck.Match(url);
                    if (match.Success) lowerScheme = match.Groups[1].Value.ToLower();
                    else lowerScheme = "#";
                }
                else
                {
                    var uri = new Uri(url);
                    lowerScheme = uri.Scheme.ToLower();
                }

                var linkLabel = GetClassedElement<VisualElement>("link", lowerScheme);
                linkLabel.tooltip = url;
                if (isValidUri)
                {
                    linkLabel.RegisterCallback<MouseUpEvent>(evt =>
                    {
                        if (LinkInlineRenderer.SchemeLinkHandlers.ContainsKey(lowerScheme))
                            LinkInlineRenderer.SchemeLinkHandlers[lowerScheme]?.Invoke(url);
                    });
                }

                renderer.Push(linkLabel);
                renderer.WriteChildren(link);
                renderer.Pop();
            }

        }

    }
}

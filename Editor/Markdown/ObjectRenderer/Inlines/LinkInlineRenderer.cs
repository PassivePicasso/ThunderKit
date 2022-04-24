using System;
using Markdig.Syntax.Inlines;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Markdig.Renderers.Html;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.ObjectRenderers
{
    using static Helpers.VisualElementFactory;
    public class LinkInlineRenderer : UIElementObjectRenderer<LinkInline>
    {
        public struct SchemeHandler
        {
            public Action<string> linkHandler;
            public Func<Label, VisualElement> preprocessor;
        }

        internal static Regex SchemeCheck = new Regex("^([\\w]+)://.*", RegexOptions.Singleline | RegexOptions.Compiled);
        internal static Dictionary<string, SchemeHandler> SchemeLinkHandlers;

        [InitializeOnLoadMethod]
        static void InitializeDefaultSchemes()
        {
            RegisterScheme("assetlink", link =>
            {
                var schemelessUri = link.Substring("assetlink://".Length);
                if (schemelessUri.Length == 0) return;
                
                string path = schemelessUri.StartsWith("GUID/") ? 
                AssetDatabase.GUIDToAssetPath(schemelessUri.Substring("GUID/".Length)) 
                : schemelessUri;

                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            },
            label =>
            {
                var schemelessUri = label.tooltip.Substring("assetlink://".Length);
                if (schemelessUri.Length == 0)
                    return label;

                string path = schemelessUri.StartsWith("GUID/") ?
                AssetDatabase.GUIDToAssetPath(schemelessUri.Substring("GUID/".Length))
                : schemelessUri;
                label.tooltip = $"assetlink://{path}";

                var container = new VisualElement();

                var icon = new Image();
                icon.AddToClassList("asset-icon");
                icon.image = AssetDatabase.GetCachedIcon(path);

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
            RegisterScheme("#", link => { });
        }

        public static bool RegisterScheme(string scheme, Action<string> action, Func<Label, VisualElement> preprocessor = null)
        {
            if (SchemeLinkHandlers == null) SchemeLinkHandlers = new Dictionary<string, SchemeHandler>();

            if (!SchemeLinkHandlers.ContainsKey(scheme.ToLowerInvariant()))
            {
                SchemeLinkHandlers[scheme.ToLowerInvariant()] = new SchemeHandler
                {
                    linkHandler = action,
                    preprocessor = preprocessor != null ? preprocessor : label => label
                };
                return true;
            }
            return false;
        }

        protected override void Write(UIElementRenderer renderer, LinkInline link)
        {
            var url = UnityWebRequest.UnEscapeURL(link.Url);
            if (link.IsImage)
            {
                var imageElement = GetImageElement<Image>(link.Url, "image");

                renderer.WriteAttributes(link.TryGetAttributes(), imageElement);
                renderer.Push(imageElement);

                renderer.WriteChildren(link);
                foreach (var child in imageElement.Children())
                    child.AddToClassList("alt-text");
                renderer.Pop();
            }
            else
            {
                var lowerScheme = string.Empty;
                var match = SchemeCheck.Match(url);
                if (match.Success) lowerScheme = match.Groups[1].Value.ToLower();

                var linkLabel = GetClassedElement<Label>(lowerScheme, "link", "inline");
                linkLabel.text = link.FirstChild.ToString();
                linkLabel.userData = url;
                linkLabel.tooltip = url;
                if (!string.IsNullOrWhiteSpace(lowerScheme))
                    linkLabel.AddToClassList(lowerScheme);

                VisualElement inlineElement = linkLabel;
                if (SchemeLinkHandlers.TryGetValue(lowerScheme, out var schemeHandlers))
                {
                    inlineElement = schemeHandlers.preprocessor(linkLabel);

                    if (match.Success)
                        linkLabel.RegisterCallback<MouseUpEvent>(evt => schemeHandlers.linkHandler?.Invoke(url));
                }
                else
                {
                    linkLabel.RegisterCallback<MouseUpEvent>(evt => schemeHandlers.linkHandler?.Invoke(url));

                }
                renderer.WriteElement(inlineElement, link);
            }

        }

    }
}

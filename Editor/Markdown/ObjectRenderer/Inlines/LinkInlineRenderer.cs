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

        protected override void Write(UIElementRenderer renderer, LinkInline link)
        {
            var url = UnityWebRequest.UnEscapeURL(link.Url);
            if (link.IsImage)
            {
                var imageElement = GetImageElement<Image>(link.Url, "image");
                var nextSibling = link.NextSibling;
                if (nextSibling != null)
                {
                    var text = nextSibling.ToString();
                    if (text.StartsWith("{") && text.EndsWith("}"))
                    {
                        // if text contains attribute size set image size
                        imageElement.AddToClassList(text.Substring(1, text.Length - 2));
                    }
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
                var match = SchemeCheck.Match(url);
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

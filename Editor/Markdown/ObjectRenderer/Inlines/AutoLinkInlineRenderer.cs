using System;
using Markdig.Syntax.Inlines;
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
    using static Helpers.UnityPathUtility;
    public class AutolinkInlineRenderer : UIElementObjectRenderer<AutolinkInline>
    {
        protected override void Write(UIElementRenderer renderer, AutolinkInline link)
        {
            var url = link.Url;
            var lowerScheme = string.Empty;
            if (link.IsEmail)
            {
                url = "mailto:" + url;
            }


            var linkLabel = GetTextElement<Label>(url, "link", lowerScheme);
            linkLabel.tooltip = url;

            renderer.WriteInline(linkLabel);
        }
    }
}

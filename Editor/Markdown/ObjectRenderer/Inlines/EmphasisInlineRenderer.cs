using Markdig.Syntax.Inlines;
using System.Collections.Generic;
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
    public class EmphasisInlineRenderer : UIElementObjectRenderer<EmphasisInline>
    {
        protected override void Write(UIElementRenderer renderer, EmphasisInline obj)
        {
            bool hasDelimiter = true;
            switch (obj.DelimiterChar)
            {
                case '*':
                    if (obj.DelimiterCount > 2) renderer.Push(GetClassedElement<VisualElement>("italic", "bold"));
                    if (obj.DelimiterCount == 2) renderer.Push(GetClassedElement<VisualElement>("italic"));
                    else renderer.Push(GetClassedElement<VisualElement>("bold"));
                    break;
                case '_' when obj.DelimiterCount == 2:
                    renderer.Push(GetClassedElement<VisualElement>("italic"));
                    break;
                default:
                    hasDelimiter = false;
                    break;
            }

            renderer.WriteChildren(obj);

            if (hasDelimiter)
                renderer.Pop();
        }
    }
}

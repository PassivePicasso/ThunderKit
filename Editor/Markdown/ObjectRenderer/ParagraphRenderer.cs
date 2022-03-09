using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Text;
using Markdig.Renderers.Html;
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
    public class ParagraphRenderer : UIElementObjectRenderer<ParagraphBlock>
    {
        
        protected override void Write(UIElementRenderer renderer, ParagraphBlock block)
        {
            VisualElement paragraph = GetClassedElement<VisualElement>("paragraph");
            renderer.Push(paragraph);
            renderer.WriteAttributes(block.TryGetAttributes(), paragraph);
            renderer.WriteOptimizedLeafBlock(block);
            renderer.Pop();
        }
    }
}

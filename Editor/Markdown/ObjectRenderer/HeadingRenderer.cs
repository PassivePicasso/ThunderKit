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
    public class HeadingRenderer : UIElementObjectRenderer<HeadingBlock>
    {
        protected override void Write(UIElementRenderer renderer, HeadingBlock block)
        {
            VisualElement header = GetClassedElement<VisualElement>($"header-{block.Level}");
            renderer.WriteAttributes(block.TryGetAttributes(), header);
            renderer.Push(header);
            var glyph = new VisualElement();
            glyph.AddToClassList("glyph");
            header.Add(glyph);
            renderer.WriteOptimizedLeafBlock(block);
            renderer.Pop();
        }
    }
}

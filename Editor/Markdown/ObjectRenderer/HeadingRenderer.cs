using Markdig.Syntax;
using Markdig.Renderers.Html;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.ObjectRenderers
{
    using static Helpers.VisualElementFactory;
    public class HeadingRenderer : UIElementObjectRenderer<HeadingBlock>
    {
        protected override void Write(UIElementRenderer renderer, HeadingBlock block)
        {
            VisualElement header = GetClassedElement<VisualElement>($"header-{block.Level}", "anchor");
            renderer.WriteAttributes(block.TryGetAttributes(), header);
            renderer.Push(header);
            header.name = GetName(block);
            header.Add(GetClassedElement<VisualElement>("glyph"));
            renderer.WriteOptimizedLeafBlock(block);
            renderer.Pop();
        }

        private static string GetName(HeadingBlock block)
        {
            Markdig.Syntax.Inlines.ContainerInline inline = block.Inline;
            var firstChild = inline.FirstChild;
            var headerValue = firstChild.ToString();
            headerValue = headerValue.ToLowerInvariant();
            headerValue = headerValue.Replace(" ", "-").Replace("\r", string.Empty).Replace("\n", string.Empty);
            var cleaned = headerValue.Replace("--", "-");
            while (cleaned != headerValue)
            {
                headerValue = cleaned;
                cleaned = headerValue.Replace("--", "-");
            }
            headerValue = cleaned;
            return headerValue;
        }
    }
}

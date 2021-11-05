using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Text;
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
            renderer.Push(GetClassedElement<VisualElement>($"header-{block.Level}"));
            renderer.WriteOptimizedLeafBlock(block);
            renderer.Pop();
        }
    }
}

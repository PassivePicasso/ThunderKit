using Markdig.Syntax.Inlines;

namespace ThunderKit.Markdown.ObjectRenderers
{
    public class LiteralInlineRenderer : UIElementObjectRenderer<LiteralInline>
    {
        
        protected override void Write(UIElementRenderer renderer, LiteralInline obj)
        {
            if (obj.Content.IsEmpty)
                return;

            renderer.WriteSplitText(ref obj.Content);
        }
    }
}

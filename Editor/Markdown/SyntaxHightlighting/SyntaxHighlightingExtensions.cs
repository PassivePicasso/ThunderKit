using ColorCode;
using Markdig;

namespace ThunderKit.Markdown.SyntaxHighlighting
{
    public static class SyntaxHighlightingExtensions
    {
        public static MarkdownPipelineBuilder UseSyntaxHighlighting(this MarkdownPipelineBuilder pipeline, IStyleSheet customCss = null)
        {
            pipeline.Extensions.Add(new SyntaxHighlightingExtension(customCss));
            return pipeline;
        }
    }
}
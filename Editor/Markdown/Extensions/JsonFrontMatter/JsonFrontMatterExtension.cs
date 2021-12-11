using Markdig;
using Markdig.Renderers;

namespace ThunderKit.Markdown.Extensions.Json
{
    /// <summary>
    /// Extension to discard a YAML frontmatter at the beginning of a Markdown document.
    /// </summary>
    public class JsonFrontMatterExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.BlockParsers.Contains<YamlFrontMatterParser>())
            {
                // Insert the YAML parser before the thematic break parser, as it is also triggered on a --- dash
                //pipeline.BlockParsers.InsertBefore<ThematicBreakParser>(new YamlFrontMatterParser());
                pipeline.BlockParsers.Insert(0, new YamlFrontMatterParser());
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (!renderer.ObjectRenderers.Contains<JsonFrontMatterRenderer>())
            {
                renderer.ObjectRenderers.Insert(0, new JsonFrontMatterRenderer());
            }
        }
    }
}

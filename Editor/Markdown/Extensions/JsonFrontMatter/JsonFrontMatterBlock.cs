using Markdig.Parsers;
using Markdig.Syntax;

namespace ThunderKit.Markdown.Extensions.Json
{
    public class JsonFrontMatterBlock : CodeBlock
    {
        public JsonFrontMatterBlock(BlockParser parser) : base(parser)
        {
        }
    }
}

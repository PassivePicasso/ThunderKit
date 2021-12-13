using Markdig;
using Markdig.Extensions.GenericAttributes;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace ThunderKit.Markdown.Extensions.GenericAttributes
{
    public class GenericAttributesExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.InlineParsers.Contains<GenericAttributesParser>())
            {
                pipeline.InlineParsers.Insert(0, new GenericAttributesParser());
            }

            foreach (BlockParser blockParser in pipeline.BlockParsers)
            {
                IAttributesParseable attributesParseable = blockParser as IAttributesParseable;
                if (attributesParseable != null)
                {
                    attributesParseable.TryParseAttributes = TryProcessAttributesForHeading;
                }
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
        }

        private bool TryProcessAttributesForHeading(BlockProcessor processor, ref StringSlice line, IBlock block)
        {
            if (line.Start < line.End)
            {
                int num = line.IndexOf('{');
                if (num >= 0)
                {
                    StringSlice slice = line;
                    slice.Start = num;
                    int start = slice.Start;
                    if (GenericAttributesParser.TryParse(ref slice, out HtmlAttributes attributes))
                    {
                        HtmlAttributes attributes2 = block.GetAttributes();
                        attributes.CopyTo(attributes2);
                        attributes2.Line = processor.LineIndex;
                        attributes2.Column = start - processor.CurrentLineStartPosition;
                        attributes2.Span.Start = start;
                        attributes2.Span.End = slice.Start - 1;
                        line.End = num - 1;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
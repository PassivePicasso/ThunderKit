using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace ThunderKit.Markdown.Extensions.Json
{
    public class YamlFrontMatterParser : BlockParser
    {
        public YamlFrontMatterParser()
        {
            base.OpeningCharacters = new char[1]
            {
                '-'
            };
        }

        protected virtual JsonFrontMatterBlock CreateFrontMatterBlock(BlockProcessor processor)
        {
            return new JsonFrontMatterBlock(this);
        }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            if (processor.IsCodeIndent)
            {
                return BlockState.None;
            }

            if (processor.Start != 0)
            {
                return BlockState.None;
            }

            int hyphenCount = 0;
            StringSlice line = processor.Line;
            char c = line.CurrentChar;
            while (c == '-' && hyphenCount < 4) // Find that the first 3 characters in hte line are ---
            {
                hyphenCount++;
                c = line.NextChar();
            }
            //If 3 hypens and the 4th is null or whitespace and we can trimp
            if (hyphenCount == 3 && (c == '\0' || c.IsWhitespace()) && line.TrimEnd())
            {
                bool flag = false;
                StringSlice stringSlice = new StringSlice(line.Text, line.Start, line.Text.Length - 1);
                c = stringSlice.CurrentChar;
                while (c != 0)
                {
                    c = stringSlice.NextChar();
                    if (c != '\n' && c != '\r')
                    {
                        continue;
                    }

                    char c2 = stringSlice.PeekChar();
                    if (c == '\r' && c2 == '\n')
                    {
                        c = stringSlice.NextChar();
                    }

                    switch (stringSlice.PeekChar())
                    {
                        case '-':
                            if (stringSlice.NextChar() != '-' || stringSlice.NextChar() != '-' || stringSlice.NextChar() != '-' || (stringSlice.NextChar() != 0 && !stringSlice.SkipSpacesToEndOfLineOrEndOfDocument()))
                            {
                                continue;
                            }

                            flag = true;
                            break;
                        case '.':
                            if (stringSlice.NextChar() != '.' || stringSlice.NextChar() != '.' || stringSlice.NextChar() != '.' || (stringSlice.NextChar() != 0 && !stringSlice.SkipSpacesToEndOfLineOrEndOfDocument()))
                            {
                                continue;
                            }

                            flag = true;
                            break;
                        default:
                            continue;
                    }

                    break;
                }

                if (flag)
                {
                    JsonFrontMatterBlock yamlFrontMatterBlock = CreateFrontMatterBlock(processor);
                    yamlFrontMatterBlock.Column = processor.Column;
                    yamlFrontMatterBlock.Span.Start = 0;
                    yamlFrontMatterBlock.Span.End = line.Start;
                    processor.NewBlocks.Push(yamlFrontMatterBlock);
                    return BlockState.ContinueDiscard;
                }
            }

            return BlockState.None;
        }

        public override BlockState TryContinue(BlockProcessor processor, Block block)
        {
            int num = 0;
            char c = processor.CurrentChar;
            StringSlice line = processor.Line;
            if (processor.Column == 0 && (c == '-' || c == '.'))
            {
                char c2 = c;
                while (c == c2)
                {
                    c = line.NextChar();
                    num++;
                }

                if (num == 3 && !processor.IsCodeIndent && (c == '\0' || c.IsWhitespace()) && line.TrimEnd())
                {
                    block.UpdateSpanEnd(line.Start - 1);
                    return BlockState.BreakDiscard;
                }
            }

            processor.GoToColumn(processor.ColumnBeforeIndent);
            return BlockState.Continue;
        }
    }
}
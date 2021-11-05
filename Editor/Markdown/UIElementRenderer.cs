using System;
using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using ThunderKit.Markdown.ObjectRenderers;
using System.Text.RegularExpressions;
using System.Text;
#if !NET40
using System.Runtime.CompilerServices;
#endif
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif
namespace ThunderKit.Markdown
{
    using static Helpers.VisualElementFactory;
    public class UIElementRenderer : RendererBase
    {
        private static readonly Regex LiteralSplitter = new Regex(@"([\S]+\b)\S?", RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly Stack<VisualElement> stack = new Stack<VisualElement>(128);

        public UIElementRenderer() { }
        public virtual void LoadDocument(VisualElement document)
        {
            Document = document;
            stack.Push(document);
            LoadRenderers();
        }

        public VisualElement Document { get; protected set; }
        /// <inheritdoc/>
        public override object Render(MarkdownObject markdownObject)
        {
            Write(markdownObject);
            return Document;
        }

        public void WriteLeafRawLines(LeafBlock leafBlock)
        {
            if (leafBlock == null) throw new ArgumentNullException(nameof(leafBlock));
            if (leafBlock.Lines.Lines != null)
            {
                var lines = leafBlock.Lines;
                var slices = lines.Lines;
                for (var i = 0; i < lines.Count; i++)
                {
                    if (i != 0)
                        WriteInline(GetClassedElement<Label>("linebreak"));

                    WriteText(ref slices[i].Slice);
                }
            }
        }
        public void Push(VisualElement o)
        {
            stack.Push(o);
        }
        public void Pop()
        {
            var popped = stack.Pop();
            stack.Peek().Add(popped);
        }
        public void WriteInline(VisualElement inline)
        {
            stack.Peek().Add(inline);
        }
        public void WriteText(ref StringSlice slice)
        {
            if (slice.IsEmpty)
                return;
            var result = slice.Text.Substring(slice.Start, slice.Length);
            var element = GetTextElement<Label>(result, "inline");
            WriteInline(element);
        }
        public void WriteSplitText(ref StringSlice slice)
        {
            if (slice.IsEmpty)
                return;

            var match = LiteralSplitter.Match(slice.Text, slice.Start, slice.Length);
            while (match.Success)
            {
                string value = match.Value;
                if (!string.IsNullOrEmpty(value))
                {
                    var element = GetTextElement<Label>(value, "inline");

                    match = match.NextMatch();

                    WriteInline(element);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns>true if constructed optimized label, otherwise false</returns>
        public void WriteOptimizedLeafInline(LeafBlock block)
        {
            var inline = block.Inline.FirstChild;
            Inline previousInline = null;
            var splitLiterals = false;
            var builder = new StringBuilder();
            while (inline != null && inline != previousInline)
            {
                previousInline = inline;
                switch (inline)
                {
                    case CodeInline _:
                    case LinkInline _:
                    case EmphasisInline _:
                        splitLiterals = true;
                        break;
                    case LineBreakInline _:
                        builder.AppendLine();
                        break;
                    case HtmlInline htmlInline:
                        builder.Append(htmlInline.Tag);
                        break;
                    default:
                        builder.Append($"{inline}");
                        break;
                }
                inline = inline.NextSibling;
                if (splitLiterals) break;
            }
            if (!splitLiterals)
            {
                var result = builder.ToString().Replace("\r\n", " ");
                var element = GetTextElement<Label>(result, "inline");
                WriteInline(element);
            }
            else
            {
                stack.Peek().AddToClassList("split-literals");
                var leafInline = block.Inline.FirstChild;
                while (leafInline != null)
                {
                    switch (leafInline)
                    {
                        case HtmlInline htmlInline:
                            WriteText(htmlInline.Tag);
                            break;
                        case HtmlEntityInline htmlEntityInline:
                            WriteText(htmlEntityInline.ToString());
                            break;
                        default:
                            Write(leafInline);
                            break;
                    }
                    leafInline = leafInline.NextSibling;
                }
            }
        }

        public void WriteText(string text)
        {
            if (text == null)
                return;

            var slice = new StringSlice(text);
            WriteSplitText(ref slice);
        }

        protected virtual void LoadRenderers()
        {
            // Default block renderers
            ObjectRenderers.Add(new CodeBlockRenderer());
            ObjectRenderers.Add(new ListRenderer());
            ObjectRenderers.Add(new HeadingRenderer());
            ObjectRenderers.Add(new ParagraphRenderer());
            ObjectRenderers.Add(new QuoteBlockRenderer());
            ObjectRenderers.Add(new ThematicBreakRenderer());

            // Default inline renderers
            ObjectRenderers.Add(new CodeInlineRenderer());
            ObjectRenderers.Add(new DelimiterInlineRenderer());
            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new LineBreakInlineRenderer());
            ObjectRenderers.Add(new LinkInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());

            // Extension renderers
            ObjectRenderers.Add(new TaskListRenderer());
        }
    }
}

using System;
using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Renderers;

#if !NET40
using System.Runtime.CompilerServices;
#endif
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using Markdig.Syntax.Inlines;
using ThunderKit.Markdown.ObjectRenderers;
using System.Text.RegularExpressions;
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
        private static Regex LiteralSplitter = new Regex("^([\\S]+\\b\\S?)|^\\s+", RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly Stack<VisualElement> stack = new Stack<VisualElement>();
        private char[] buffer;

        public UIElementRenderer()
        {
            buffer = new char[1024];
        }

        public UIElementRenderer(VisualElement document)
        {
            buffer = new char[1024];
            LoadDocument(document);
        }

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

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteLeafInline(LeafBlock leafBlock)
        {
            if (leafBlock == null) throw new ArgumentNullException(nameof(leafBlock));
            var inline = (Inline)leafBlock.Inline;
            while (inline != null)
            {
                Write(inline);
                inline = inline.NextSibling;
            }
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

        public void WriteBlock(VisualElement block)
        {
            stack.Peek().Add(block);
        }

        public void WriteInline(VisualElement inline)
        {
            AddInline(stack.Peek(), inline);
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteText(ref StringSlice slice)
        {
            if (slice.Start > slice.End)
                return;

            WriteText(slice.Text, slice.Start, slice.Length);
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteText(string text)
        {
            var content = text;
            int safetyBreak = 0;
            while (++safetyBreak < 10 && !string.IsNullOrWhiteSpace(content) && content.Length > 0)
            {
                var match = LiteralSplitter.Match(content);
                if (match.Success)
                {
                    if (!string.IsNullOrEmpty(match.Value) && !string.IsNullOrWhiteSpace(match.Value))
                    {
                        safetyBreak = 0;
                        content = content.Substring(match.Value.Length);
                        WriteInline(GetTextElement<Label>(match.Value, "inline"));
                    }
                    else
                        content = content.Substring(1);
                }
                else
                    break;
            }
        }

        public void WriteText(string text, int offset, int length)
        {
            if (text == null)
                return;

            if (offset == 0 && text.Length == length)
            {
                WriteText(text);
            }
            else
            {
                if (length > buffer.Length)
                {
                    buffer = text.ToCharArray();
                    WriteText(new string(buffer, offset, length));
                }
                else
                {
                    text.CopyTo(offset, buffer, 0, length);
                    WriteText(new string(buffer, 0, length));
                }
            }
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
            ObjectRenderers.Add(new AutolinkInlineRenderer());
            ObjectRenderers.Add(new CodeInlineRenderer());
            ObjectRenderers.Add(new DelimiterInlineRenderer());
            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new HtmlEntityInlineRenderer());
            ObjectRenderers.Add(new LineBreakInlineRenderer());
            ObjectRenderers.Add(new LinkInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());

            // Extension renderers
            //ObjectRenderers.Add(new TableRenderer());
            ObjectRenderers.Add(new TaskListRenderer());
        }

        private static void AddInline(VisualElement parent, VisualElement inline)
        {
            parent.Add(inline);
        }
    }
}

using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using ThunderKit.Markdown.ObjectRenderers;
using System.Text;
#if !NET40
#endif
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif


namespace ThunderKit.Markdown
{
    using HtmlAttributesExtensions = Markdig.Renderers.Html.HtmlAttributesExtensions;
    using HtmlAttributes = Markdig.Renderers.Html.HtmlAttributes;
    using static Helpers.VisualElementFactory;
    public class UIElementRenderer : RendererBase
    {
        private readonly Stack<VisualElement> stack = new Stack<VisualElement>(128);

        public UIElementRenderer() { }
        public void LoadDocument(MarkdownElement document)
        {
            Document = document;
            stack.Clear();
            stack.Push(document);
            LoadRenderers();
        }

        public MarkdownElement Document { get; protected set; }
        /// <inheritdoc/>
        public override object Render(MarkdownObject markdownObject)
        {
            Write(markdownObject);
            return Document;
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

        internal VisualElement Peek()
        {
            return stack.Peek();
        }

        public void WriteElement(VisualElement element, MarkdownObject mdo = null)
        {
            stack.Peek().Add(element);
            if (mdo != null)
                WriteAttributes(HtmlAttributesExtensions.TryGetAttributes(mdo), element);
        }
        public void WriteSplitText(ref StringSlice slice)
        {
            if (slice.IsEmpty)
                return;

            var text = slice.ToString();
            for (int i = 0; i < text.Length;)
            {
                int nextI = text.IndexOf(' ', i + 1);
                if (nextI == i) break;

                string value;
                if (nextI == -1)
                    value = text.Substring(i);
                else
                    value = text.Substring(i, nextI - i);

                value = value.Trim(' ', '\r', '\n');
                i = nextI;

                if (!string.IsNullOrEmpty(value))
                    WriteElement(GetTextElement<Label>(value, "inline"));

                if (i == -1) break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns>true if constructed optimized label, otherwise false</returns>
        public void WriteOptimizedLeafBlock(LeafBlock block)
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
                WriteElement(element);
            }
            else
            {
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
                    leafInline = leafInline?.NextSibling;
                }
            }
        }

        /// <summary>
        /// Writes the specified <see cref="HtmlAttributes"/>.
        /// </summary>
        /// <param name="attributes">The attributes to render.</param>
        /// <param name="classFilter">A class filter used to transform a class into another class at writing time</param>
        /// <returns>This instance</returns>
        public void WriteAttributes(HtmlAttributes attributes, VisualElement element)
        {
            if (attributes == null) return;

            if (attributes.Id != null)
            {
                element.name = attributes.Id;
            }

            if (attributes.Classes != null && attributes.Classes.Count > 0)
            {
                foreach (var cls in attributes.Classes)
                    element.EnableInClassList(cls, true);
            }

            if (attributes.Properties != null && attributes.Properties.Count > 0)
            {
                element.userData = attributes.Properties;
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
        }
    }
}

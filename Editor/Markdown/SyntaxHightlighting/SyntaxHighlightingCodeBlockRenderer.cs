using ColorCode;
using Markdig.Syntax;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using ThunderKit.Markdown.ObjectRenderers;
using System.IO;
using Markdig.Parsers;
using ColorCode.Parsing;
using ColorCode.Compilation;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

using Color = ColorCode.Styling.Color;
using UColor = UnityEngine.Color;
using ColorCode.Common;
using ColorCode.Styling.StyleSheets;

namespace ThunderKit.Markdown.SyntaxHighlighting
{
    using static Helpers.VisualElementFactory;
    using static Helpers.UnityPathUtility;
    public class SyntaxHighlightingCodeBlockRenderer : UIElementObjectRenderer<CodeBlock>
    {
        private readonly CodeBlockRenderer _underlyingRenderer;
        private readonly IStyleSheet _customCss;
        private readonly Languages languages;
        static readonly char[] splitChars = new char[] { ' ' };

        public SyntaxHighlightingCodeBlockRenderer(CodeBlockRenderer underlyingRenderer = null, IStyleSheet customCss = null)
        {
            _underlyingRenderer = underlyingRenderer ?? new CodeBlockRenderer();
            _customCss = customCss;
            languages = new Languages();
        }

        protected override void Write(UIElementRenderer renderer, CodeBlock obj)
        {
            var fencedCodeBlock = obj as FencedCodeBlock;
            var parser = obj.Parser as FencedCodeBlockParser;
            if (fencedCodeBlock == null || parser == null)
            {
                _underlyingRenderer.Write(renderer, obj);
                return;
            }

            var languageMoniker = fencedCodeBlock.Info.Replace(parser.InfoPrefix, string.Empty);

            renderer.Push(GetClassedElement<VisualElement>($"lang-{languageMoniker}", "editor-colors", "code"));

            if (string.IsNullOrEmpty(languageMoniker)) _underlyingRenderer.Write(renderer, obj);
            else
            {
                var code = GetCode(obj, out var firstLine);
                var languageTypeAdapter = new LanguageTypeAdapter(languages);
                var language = languageTypeAdapter.Parse(languageMoniker, firstLine);

                if (language == null)
                { //handle unrecognised language formats, e.g. when using mermaid diagrams
                    return;
                }

                var styleSheet = _customCss ?? StyleSheets.Default;
                var languageParser = new LanguageParser(new LanguageCompiler(languages.CompiledLanguages), languages.LanguageRepository);
                var newLineExtract = new Regex("^([\\S]*?)$", RegexOptions.Multiline);

                renderer.Push(GetClassedElement<VisualElement>("code-line"));
                languageParser.Parse(code, language, ProcessScope);
                if (renderer.Peek().ClassListContains("code-line")) renderer.Pop();

                void ProcessScope(string parsedSourceCode, IList<Scope> scopes)
                {
                    if (parsedSourceCode.StartsWith("\r\n"))
                    {
                        if (renderer.Peek().ClassListContains("code-line")) renderer.Pop();
                        renderer.Push(GetClassedElement<VisualElement>("code-line"));
                        var trimmedStart = parsedSourceCode.Substring(2);
                        if (trimmedStart.Length > 0)
                            ProcessScope(trimmedStart, scopes);
                        return;
                    }
                    if (scopes.Count > 0)
                    {
                        foreach (var scope in scopes)
                        {
                            var label = GetClassedElement<Label>("inline");
                            StyleLabel(label, styleSheet, scope);
                            label.text = parsedSourceCode;
                            renderer.WriteInline(label);
                        }
                    }
                    else
                    {
                        var next = parsedSourceCode.IndexOf("\r\n");
                        var hasMoreLines = next != -1;
                        next = next == -1 ? parsedSourceCode.Length : next;
                        var lineText = parsedSourceCode.Substring(0, next);
                        if (lineText.Length > 1)
                            while (lineText.StartsWith(" "))
                            {
                                renderer.WriteInline(GetTextElement<Label>(" ", "inline"));
                                lineText = lineText.Substring(1);
                            }

                        var label = GetTextElement<Label>(lineText.Trim(' '), "inline");
                        renderer.WriteInline(label);

                        if (lineText.Length > 1)
                            while (lineText.EndsWith(" "))
                            {
                                renderer.WriteInline(GetTextElement<Label>(" ", "inline"));
                                lineText = lineText.Substring(0, lineText.Length - 1);
                            }

                        if (hasMoreLines)
                            ProcessScope(parsedSourceCode.Substring(next), scopes);
                    }
                }
            }

            renderer.Pop();
        }

        void StyleLabel(Label label, IStyleSheet styleSheet, Scope scope)
        {
            var styles = styleSheet.Styles;
            var scopeName = scope?.Name;
            if (styles.Contains(scopeName))
            {
                Style style = styleSheet.Styles[scope.Name];
                if (style.Italic) label.AddToClassList("italic");
                if (style.Bold) label.AddToClassList("bold");
                if (style.Background != null)
                {
                    var bg = style.Background.ToHtmlColor();
                    if (ColorUtility.TryParseHtmlString(bg, out UColor bgColor))
                        label.style.backgroundColor = bgColor;
                }
                if (style.Foreground != null)
                {
                    var fg = style.Foreground.ToHtmlColor();
                    if (ColorUtility.TryParseHtmlString(fg, out UColor fgColor))
                        label.style.color = fgColor;
                }
            }
        }
        private static string GetCode(LeafBlock obj, out string firstLine)
        {
            var code = new StringBuilder();
            firstLine = null;
            foreach (var line in obj.Lines.Lines)
            {
                var slice = line.Slice;
                if (slice.Text == null)
                {
                    continue;
                }

                var lineText = slice.Text.Substring(slice.Start, slice.Length);

                if (firstLine == null)
                {
                    firstLine = lineText;
                }
                else
                {
                    code.AppendLine();
                }

                code.Append(lineText);
            }
            return code.ToString();
        }
    }
}

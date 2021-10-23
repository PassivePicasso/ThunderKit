using ColorCode;
using Markdig.Syntax;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using ThunderKit.Markdown.ObjectRenderers;
using System.IO;
using Markdig.Parsers;
using Markdig;
using Markdig.Renderers;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.SyntaxHighlighting
{
    public class SyntaxHighlightingExtension : IMarkdownExtension
    {
        private readonly IStyleSheet _customCss;

        public SyntaxHighlightingExtension(IStyleSheet customCss = null)
        {
            _customCss = customCss;
        }

        public void Setup(MarkdownPipelineBuilder pipeline) { }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            UIElementRenderer uieRenderer = renderer as UIElementRenderer;

            if (uieRenderer == null)
            {
                return;
            }

            var originalCodeBlockRenderer = uieRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
            if (originalCodeBlockRenderer != null)
            {
                uieRenderer.ObjectRenderers.Remove(originalCodeBlockRenderer);
            }

            uieRenderer.ObjectRenderers.AddIfNotAlready(
                new SyntaxHighlightingCodeBlockRenderer(originalCodeBlockRenderer, _customCss));
        }
    }
}
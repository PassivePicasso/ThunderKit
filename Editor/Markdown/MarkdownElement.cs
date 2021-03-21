using Markdig;
using Markdig.Extensions.GenericAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Markdig.Renderers.Normalize;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown
{
    using static Helpers.UnityPathUtility;

    public enum MarkdownDataType { Implicit, Source, Text }
    public class MarkdownElement : VisualElement
    {
        private static event EventHandler UpdateMarkdown;

        private static UIElementRenderer renderer;
        static MarkdownElement()
        {
            renderer = new UIElementRenderer();
            var mpb = new MarkdownPipelineBuilder();
            mpb.Extensions.AddIfNotAlready<GenericAttributesExtension>();
            var pipeline = mpb.Build();
            pipeline.Setup(renderer);
            EditorApplication.projectChanged += EditorApplication_projectChanged;
        }
        private static void EditorApplication_projectChanged()
        {
            UpdateMarkdown?.Invoke(null, EventArgs.Empty);
        }
        public string Data { get; set; }
        public MarkdownDataType MarkdownDataType { get; set; }
        public bool SpaceAfterQuoteBlock { get; set; }
        public bool EmptyLineAfterCodeBlock { get; set; }
        public bool EmptyLineAfterHeading { get; set; }
        public bool EmptyLineAfterThematicBreak { get; set; }
        public string ListItemCharacter { get; set; } = "*";
        public bool ExpandAutoLinks { get; set; }
        public MarkdownElement()
        {
        }

        void RefreshContent(string markdown)
        {
            var normalizeOptions = new NormalizeOptions
            {
                EmptyLineAfterCodeBlock = EmptyLineAfterCodeBlock,
                EmptyLineAfterHeading = EmptyLineAfterHeading,
                EmptyLineAfterThematicBreak = EmptyLineAfterThematicBreak,
                ExpandAutoLinks = ExpandAutoLinks,
                ListItemCharacter = ListItemCharacter[0],
                SpaceAfterQuoteBlock = SpaceAfterQuoteBlock
            };

            markdown = Markdig.Markdown.Normalize(markdown, normalizeOptions);

            var document = Markdig.Markdown.Parse(markdown);
            renderer.LoadDocument(this);
            renderer.Render(document);
        }

        public new class UxmlFactory : UxmlFactory<MarkdownElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            static string NormalizeName(string text) => ObjectNames.NicifyVariableName(text).ToLower();

            private UxmlStringAttributeDescription m_text = new UxmlStringAttributeDescription { name = "data" };
            private UxmlEnumAttributeDescription<MarkdownDataType> m_dataType = new UxmlEnumAttributeDescription<MarkdownDataType> { name = "markdown-data-type" };
            private UxmlBoolAttributeDescription m_EmptyLineAfterCodeBlock = new UxmlBoolAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.EmptyLineAfterCodeBlock)), defaultValue = true };
            private UxmlBoolAttributeDescription m_EmptyLineAfterHeading = new UxmlBoolAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.EmptyLineAfterHeading)), defaultValue = true };
            private UxmlBoolAttributeDescription m_EmptyLineAfterThematicBreak = new UxmlBoolAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.EmptyLineAfterThematicBreak)), defaultValue = true };
            private UxmlBoolAttributeDescription m_ExpandAutoLinks = new UxmlBoolAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.ExpandAutoLinks)), defaultValue = true };
            private UxmlStringAttributeDescription m_ListItemCharacter = new UxmlStringAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.ListItemCharacter)), defaultValue = "*" };
            private UxmlBoolAttributeDescription m_SpaceAfterQuoteBlock = new UxmlBoolAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.SpaceAfterQuoteBlock)), defaultValue = true };
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var mdElement = (MarkdownElement)ve;
                mdElement.Clear();
                mdElement.Data = m_text.GetValueFromBag(bag, cc);
                mdElement.MarkdownDataType = m_dataType.GetValueFromBag(bag, cc);
                mdElement.EmptyLineAfterCodeBlock = m_EmptyLineAfterCodeBlock.GetValueFromBag(bag, cc);
                mdElement.EmptyLineAfterHeading = m_EmptyLineAfterHeading.GetValueFromBag(bag, cc);
                mdElement.EmptyLineAfterThematicBreak = m_EmptyLineAfterThematicBreak.GetValueFromBag(bag, cc);
                mdElement.ExpandAutoLinks = m_ExpandAutoLinks.GetValueFromBag(bag, cc);
                mdElement.ListItemCharacter = m_ListItemCharacter.GetValueFromBag(bag, cc);
                mdElement.SpaceAfterQuoteBlock = m_SpaceAfterQuoteBlock.GetValueFromBag(bag, cc);

                var markdown = string.Empty;
                switch (mdElement.MarkdownDataType)
                {
                    case MarkdownDataType.Implicit:
                        {
                            if (cc.visualTreeAsset == null) break;
                            var treeAssetPath = AssetDatabase.GetAssetPath(cc.visualTreeAsset);
                            if (string.IsNullOrEmpty(treeAssetPath)) break;
                            var treeAssetDirectory = Path.GetDirectoryName(treeAssetPath);
                            var fileName = $"{Path.GetFileNameWithoutExtension(treeAssetPath)}.md";
                            var sourcePath = Path.Combine(treeAssetDirectory, fileName);
                            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(sourcePath);
                            markdown = asset?.text ?? string.Empty;
                        }
                        break;
                    case MarkdownDataType.Source:
                        var source = mdElement.Data;
                        if (!".md".Equals(Path.GetExtension(source))) break;

                        if (IsAssetDirectory(source))
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(source);
                            markdown = asset?.text ?? string.Empty;
                        }
                        else
                        {
                            var treeAssetPath = AssetDatabase.GetAssetPath(cc.visualTreeAsset);
                            var treeAssetDirectory = Path.GetDirectoryName(treeAssetPath);
                            var sourcePath = Path.Combine(treeAssetDirectory, source);
                            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(sourcePath);
                            markdown = asset?.text ?? string.Empty;
                        }
                        break;
                    case MarkdownDataType.Text:
                        markdown = mdElement.Data;
                        break;
                }
                if (markdown == string.Empty)
                {
                    mdElement.Add(new Label($"No data found: {mdElement.MarkdownDataType} : {mdElement.Data}"));
                    return;
                }
                mdElement.RefreshContent(markdown);
            }

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get
                {
                    yield break;
                }
            }
        }
    }
}
using Markdig;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Markdig.Renderers.Normalize;
using System.Linq;
using ThunderKit.Markdown.Extensions.Json;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown
{
    using static Helpers.UnityPathUtility;

    public enum MarkdownDataType { Implicit, Source, Text }
    public class MarkdownElement : VisualElement
    {
        const string MarkdownStylePath = "Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss";
        private readonly UIElementRenderer renderer;
        private readonly MarkdownPipelineBuilder mpb;
        private readonly MarkdownPipeline pipeline;
        private string data, prevData;
        private FileSystemWatcher fileSystemWatcher;
        public string Data
        {
            get => data; set
            {
                prevData = data;
                data = value;
            }
        }
        private string Markdown { get; set; }
        private string NormalizedMarkdown { get; set; }
        public MarkdownDataType MarkdownDataType { get; set; }
        public bool SpaceAfterQuoteBlock { get; set; }
        public bool EmptyLineAfterCodeBlock { get; set; }
        public bool EmptyLineAfterHeading { get; set; }
        public bool EmptyLineAfterThematicBreak { get; set; }
        public string ListItemCharacter { get; set; } = "*";
        public bool ExpandAutoLinks { get; set; }
        public MarkdownElement()
        {
            renderer = new UIElementRenderer();
            mpb = new MarkdownPipelineBuilder();
            mpb.Extensions.AddIfNotAlready<JsonFrontMatterExtension>();
            mpb.DisableHtml();
            pipeline = mpb.Build();
            pipeline.Setup(renderer);

            AddSheet(MarkdownStylePath);
            if (EditorGUIUtility.isProSkin)
                AddSheet(MarkdownStylePath, "Dark");

            MarkdownFileWatcher.DocumentUpdated += MarkdownFileWatcher_DocumentUpdated;
#if UNITY_2021_1_OR_NEWER
            AddSheet(MarkdownStylePath, "2021");
#elif UNITY_2020_1_OR_NEWER
            AddSheet(MarkdownStylePath, "2020");
#elif UNITY_2019_1_OR_NEWER
            AddSheet(MarkdownStylePath, "2019");
#elif UNITY_2018_1_OR_NEWER
            AddSheet(MarkdownStylePath, "2018");
#endif
        }

        private void MarkdownFileWatcher_DocumentUpdated(object sender, (string path, MarkdownFileWatcher.ChangeType change) e)
        {
            if(e.path == Data && e.change == MarkdownFileWatcher.ChangeType.Imported)
            {
                RefreshContent();
            }
        }

        public void AddSheet(string templatePath, string modifier = null)
        {
            string path;

            if (!string.IsNullOrEmpty(modifier))
            {
                path = templatePath.Replace(".uss", $"_{modifier}.uss");
                if (!File.Exists(path))
                {
                    path = templatePath.Replace(".uss", $"_{modifier}.uss");
                    if (!File.Exists(path))
                        return;
                }
            }
            else path = templatePath;

            MultiVersionLoadStyleSheet(path);
        }

        public void MultiVersionLoadStyleSheet(string sheetPath)
        {
#if UNITY_2019_1_OR_NEWER
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(sheetPath);
            if (!styleSheets.Contains(styleSheet))
                styleSheets.Add(styleSheet);
#elif UNITY_2018_1_OR_NEWER
            if (!HasStyleSheetPath(sheetPath))
                AddStyleSheetPath(sheetPath);
#endif
        }

        string GetMarkdown()
        {
            string markdown = string.Empty;
            switch (MarkdownDataType)
            {
                case MarkdownDataType.Implicit:
                case MarkdownDataType.Source:
                    if (!".md".Equals(Path.GetExtension(Data))) break;

                    var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(Data);
                    if (asset)
                        markdown = asset.text;
                    else
                        markdown = string.Empty;

                    break;
                case MarkdownDataType.Text:
                    markdown = Data;
                    break;
            }

            return string.IsNullOrEmpty(markdown) ? $"No data found: {MarkdownDataType} : {Data}" : markdown;
        }

        public void RefreshContent()
        {
            var markdown = GetMarkdown();
            if (markdown.Equals(Markdown)) return;
            Markdown = markdown;

            Clear();

            var normalizeOptions = new NormalizeOptions
            {
                EmptyLineAfterCodeBlock = EmptyLineAfterCodeBlock,
                EmptyLineAfterHeading = EmptyLineAfterHeading,
                EmptyLineAfterThematicBreak = EmptyLineAfterThematicBreak,
                ExpandAutoLinks = ExpandAutoLinks,
                ListItemCharacter = ListItemCharacter[0],
                SpaceAfterQuoteBlock = SpaceAfterQuoteBlock
            };


            NormalizedMarkdown = Markdig.Markdown.Normalize(Markdown, normalizeOptions);

            var document = Markdig.Markdown.Parse(NormalizedMarkdown, pipeline);
            renderer.LoadDocument(this);
            renderer.Render(document);
        }

        public new class UxmlFactory : UxmlFactory<MarkdownElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            static string NormalizeName(string text) => ObjectNames.NicifyVariableName(text).ToLower();

            private readonly UxmlStringAttributeDescription m_text = new UxmlStringAttributeDescription { name = "data" };
            private readonly UxmlEnumAttributeDescription<MarkdownDataType> m_dataType = new UxmlEnumAttributeDescription<MarkdownDataType> { name = "markdown-data-type" };
            private readonly UxmlBoolAttributeDescription m_EmptyLineAfterCodeBlock = new UxmlBoolAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.EmptyLineAfterCodeBlock)), defaultValue = true };
            private readonly UxmlBoolAttributeDescription m_EmptyLineAfterHeading = new UxmlBoolAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.EmptyLineAfterHeading)), defaultValue = true };
            private readonly UxmlBoolAttributeDescription m_EmptyLineAfterThematicBreak = new UxmlBoolAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.EmptyLineAfterThematicBreak)), defaultValue = true };
            private readonly UxmlBoolAttributeDescription m_ExpandAutoLinks = new UxmlBoolAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.ExpandAutoLinks)), defaultValue = true };
            private readonly UxmlStringAttributeDescription m_ListItemCharacter = new UxmlStringAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.ListItemCharacter)), defaultValue = "*" };
            private readonly UxmlBoolAttributeDescription m_SpaceAfterQuoteBlock = new UxmlBoolAttributeDescription { name = NormalizeName(nameof(NormalizeOptions.SpaceAfterQuoteBlock)), defaultValue = true };
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var mdElement = (MarkdownElement)ve;
                mdElement.Data = m_text.GetValueFromBag(bag, cc);
                mdElement.MarkdownDataType = m_dataType.GetValueFromBag(bag, cc);
                mdElement.EmptyLineAfterCodeBlock = m_EmptyLineAfterCodeBlock.GetValueFromBag(bag, cc);
                mdElement.EmptyLineAfterHeading = m_EmptyLineAfterHeading.GetValueFromBag(bag, cc);
                mdElement.EmptyLineAfterThematicBreak = m_EmptyLineAfterThematicBreak.GetValueFromBag(bag, cc);
                mdElement.ExpandAutoLinks = m_ExpandAutoLinks.GetValueFromBag(bag, cc);
                mdElement.ListItemCharacter = m_ListItemCharacter.GetValueFromBag(bag, cc);
                mdElement.SpaceAfterQuoteBlock = m_SpaceAfterQuoteBlock.GetValueFromBag(bag, cc);

                bool configured = false;
                if (mdElement.MarkdownDataType != MarkdownDataType.Text)
                {
                    if (IsAssetDirectory(mdElement.Data)) configured = true;
                    else if (cc.visualTreeAsset != null)
                    {
                        var treeAssetPath = AssetDatabase.GetAssetPath(cc.visualTreeAsset);
                        if (!string.IsNullOrEmpty(treeAssetPath))
                        {
                            var treeAssetDirectory = Path.GetDirectoryName(treeAssetPath);
                            var source = string.IsNullOrEmpty(mdElement.Data) ? $"{Path.GetFileNameWithoutExtension(treeAssetPath)}.md"
                                                                              : mdElement.Data;
                            var sourcePath = Path.Combine(treeAssetDirectory, source);
                            mdElement.Data = sourcePath;
                            configured = true;
                        }
                    }
                }
                else
                    configured = true;

                if (configured)
                    mdElement.RefreshContent();
            }

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription =>
                Enumerable.Empty<UxmlChildElementDescription>();
        }
    }
}
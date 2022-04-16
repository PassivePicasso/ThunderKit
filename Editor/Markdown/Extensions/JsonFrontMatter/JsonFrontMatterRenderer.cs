using ThunderKit.Markdown.ObjectRenderers;
using UnityEngine;
using System;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.Extensions.Json
{
    using static Helpers.VisualElementFactory;
    public class JsonFrontMatterRenderer : UIElementObjectRenderer<JsonFrontMatterBlock>
    {
        /// <summary>
        /// Pre-Defined FontMatter for ThunderKit Documentation files
        /// </summary>
        public struct FrontMatter
        {
            /// <summary>
            /// Stylesheet to load into root MarkdownElement
            /// </summary>
            public string pageStylePath;

            /// <summary>
            /// Classes to apply to JsonFrontMatter rendered Header container
            /// </summary>
            public string[] headerClasses;

            /// <summary>
            /// Title to display in Header's Label
            /// setting title enables the display of the Page Header
            /// </summary>
            public string title;
            /// <summary>
            /// Classes to apply to the Title label in the header
            /// </summary>
            public string[] titleClasses;

            /// <summary>
            /// Classes to apply to the Icon VisualElement in the page Header
            /// </summary>
            public string[] iconClasses;

            /// <summary>
            /// Url of content to be rendered at the end of the current MarkdownElement
            /// </summary>
            public string contentUrl;
        }
        protected override void Write(UIElementRenderer renderer, JsonFrontMatterBlock frontMatterBlock)
        {
            try
            {
                var json = frontMatterBlock.Lines.ToString().Trim();
                var frontMatter = JsonUtility.FromJson<FrontMatter>(json);

                VisualElement jsonFrontMatter = new VisualElement { name = "markdown-frontmatter-container" };
                renderer.Push(jsonFrontMatter);

                if (!string.IsNullOrEmpty(frontMatter.title))
                {
                    var header = GeneratePageHeader(frontMatter);
                    renderer.WriteElement(header);
                }

                if (!string.IsNullOrEmpty(frontMatter.contentUrl))
                {
                    var markdown = new MarkdownElement { Data = frontMatter.contentUrl };
                    renderer.WriteElement(markdown);
                    markdown.RefreshContent();
                }
                if (!string.IsNullOrEmpty(frontMatter.pageStylePath))
                    MultiVersionLoadStyleSheet(jsonFrontMatter, frontMatter.pageStylePath);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                renderer.WriteElement(GetTextElement<Label>(e.Message));
            }
        }

        private VisualElement GeneratePageHeader(FrontMatter frontMatter)
        {
            var header = GetClassedElement<VisualElement>(frontMatter.headerClasses);
            header.name = nameof(header);

            var headerIcon = GetClassedElement<VisualElement>(frontMatter.iconClasses);
            headerIcon.name = nameof(headerIcon);
            header.Add(headerIcon);

            if (!string.IsNullOrEmpty(frontMatter.title))
            {
                var title = GetTextElement<Label>(frontMatter.title, frontMatter.titleClasses);
                title.name = nameof(title);
                header.Add(title);
            }

            return header;
        }

        public void MultiVersionLoadStyleSheet(VisualElement element, string sheetPath)
        {
#if UNITY_2019_1_OR_NEWER
            var styleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(sheetPath);
            if (!element.styleSheets.Contains(styleSheet))
                element.styleSheets.Add(styleSheet);
#elif UNITY_2018_1_OR_NEWER
            if (!element.HasStyleSheetPath(sheetPath))
                element.AddStyleSheetPath(sheetPath);
#endif
        }
    }
}

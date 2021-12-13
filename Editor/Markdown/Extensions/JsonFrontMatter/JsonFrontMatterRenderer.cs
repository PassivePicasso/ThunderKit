using ThunderKit.Markdown.ObjectRenderers;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
using System;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.Extensions.Json
{
    using static Helpers.VisualElementFactory;
    using static Helpers.UnityPathUtility;
    public class JsonFrontMatterRenderer : UIElementObjectRenderer<JsonFrontMatterBlock>
    {
        public struct FrontMatter
        {
            public string pageStylePath;

            public string[] headerClasses;

            public string title;
            public string[] titleClasses;

            public string[] iconClasses;

            public string contentUrl;
        }
        protected override void Write(UIElementRenderer renderer, JsonFrontMatterBlock frontMatterBlock)
        {
            try
            {
                var json = frontMatterBlock.Lines.ToString().Trim();
                var frontMatter = JsonUtility.FromJson<FrontMatter>(json);

                if (!string.IsNullOrEmpty(frontMatter.title))
                {
                    var header = GeneratePageHeader(frontMatter);
                    renderer.WriteElement(header);
                }

                if (!string.IsNullOrEmpty(frontMatter.contentUrl)
                    && renderer.Peek() is MarkdownElement markdown
                    && markdown.Data != frontMatter.contentUrl)
                {
                    markdown.Data = frontMatter.contentUrl;
                    markdown.RefreshContent();
                }

                var parent = renderer.Peek();
                if (!parent.HasStyleSheetPath(frontMatter.pageStylePath))
                    parent.AddStyleSheetPath(frontMatter.pageStylePath);
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
    }
}

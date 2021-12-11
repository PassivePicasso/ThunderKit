using ThunderKit.Markdown.ObjectRenderers;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
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

            public string content;
            public string[] contentClasses;

            public string iconUrl;
            public string[] iconClasses;
        }
        protected override void Write(UIElementRenderer renderer, JsonFrontMatterBlock frontMatterBlock)
        {
            var json = frontMatterBlock.Lines.ToString();
            var frontMatter = JsonUtility.FromJson<FrontMatter>(json);

            var header = GetClassedElement<VisualElement>(frontMatter.headerClasses);
            header.name = nameof(header);

            if (!string.IsNullOrEmpty(frontMatter.iconUrl))
            {
                var headerIcon = GetImageElement<Image>(frontMatter.iconUrl, frontMatter.iconClasses);
                headerIcon.name = nameof(headerIcon);
                header.Add(headerIcon);
            }
            if (!string.IsNullOrEmpty(frontMatter.content))
            {
                var title = GetTextElement<Label>(frontMatter.content, frontMatter.contentClasses);
                title.name = nameof(title);
                header.Add(title);
            }
            else if (IsAssetDirectory(renderer.Document.Data))
            {
                var fileName = Path.GetFileNameWithoutExtension(renderer.Document.Data);
                fileName = ObjectNames.NicifyVariableName(fileName);
                var title = GetTextElement<Label>(fileName, frontMatter.contentClasses);
                title.name = nameof(title);
                header.Add(title);
            }

            renderer.WriteElement(header);
            var parent = header.parent;
            if (!parent.HasStyleSheetPath(frontMatter.pageStylePath))
                parent.AddStyleSheetPath(frontMatter.pageStylePath);
        }
    }
}

using Markdig.Syntax;
#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.ObjectRenderers
{
    using static Helpers.VisualElementFactory;
    public class CodeBlockRenderer : UIElementObjectRenderer<CodeBlock>
    {
        protected override void Write(UIElementRenderer renderer, CodeBlock obj)
        {
            var lines = obj.Lines;
            var slices = lines.Lines;
            var result = string.Empty;
            for (var i = 0; i < lines.Count; i++)
            {
                if (i != 0)
                    result += "\r\n";

                var slice = slices[i].Slice;

                result += slice.Text.Substring(slice.Start, slice.Length);
            }

            var container = new VisualElement();

            var block = SyntaxHighlighter.Highlight(result);
            block.AddToClassList("code");
            MultiVersionLoadStyleSheet(block, "Packages/com.passivepicasso.thunderkit/USS/csharp-syntax.uss");

            var textField = GetClassedElement<TextField>("code-hidden");
            textField.multiline = true;
            textField.value = result;
            textField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            container.Add(block);
            container.Add(textField);
            renderer.WriteElement(container, obj);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            evt.PreventDefault();
        }

        public static void MultiVersionLoadStyleSheet(VisualElement element, string sheetPath)
        {
#if UNITY_2019_1_OR_NEWER
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(sheetPath);
            if (!element.styleSheets.Contains(styleSheet))
                element.styleSheets.Add(styleSheet);
#elif UNITY_2018_1_OR_NEWER
            if (!element.HasStyleSheetPath(sheetPath))
                element.AddStyleSheetPath(sheetPath);
#endif
        }
    }
}

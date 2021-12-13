using Markdig.Syntax;
#if UNITY_2019_1_OR_NEWER
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

            var textField = GetClassedElement<TextField>("code");
            textField.multiline = true;
            textField.value = result;
            textField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            renderer.WriteElement(textField, obj);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            evt.PreventDefault();
        }
    }
}

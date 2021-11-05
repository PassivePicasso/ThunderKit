using System.Globalization;
using Markdig.Syntax;
using System.Linq;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.ObjectRenderers
{
    using static Helpers.VisualElementFactory;
    using static Helpers.UnityPathUtility;
    public class ListRenderer : UIElementObjectRenderer<ListBlock>
    {
        protected override void Write(UIElementRenderer renderer, ListBlock listBlock)
        {
            renderer.Push(GetClassedElement<VisualElement>("list"));


            foreach (var item in listBlock.OfType<ListItemBlock>())
            {
                var listItem = GetClassedElement<VisualElement>("list-item");
                var listItemChildren = GetClassedElement<VisualElement>("list-item-children");

                renderer.Push(listItem);
                {
                    var marker = listBlock.IsOrdered ? $"{item.Order}." : $"{listBlock.BulletType}";
                    var classes = listBlock.IsOrdered ? "inline" : "bullet";
                    renderer.WriteElement(GetTextElement<Label>(marker, classes));

                    renderer.Push(listItemChildren);
                    {
                        renderer.WriteChildren(item);
                    }
                    renderer.Pop();
                }
                renderer.Pop();
            }

            renderer.Pop();
        }
    }
}

using System.Globalization;
using Markdig.Syntax;
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
    using static Helpers.VisualElementUtility;
    using static Helpers.UnityPathUtility;
    public class ListRenderer : UIElementObjectRenderer<ListBlock>
    {
        protected override void Write(UIElementRenderer renderer, ListBlock listBlock)
        {
            renderer.Push(GetClassedElement<VisualElement>("list"));

            foreach (var item in listBlock)
            {
                var listItemBlock = (ListItemBlock)item;
                renderer.Push(GetClassedElement<VisualElement>("list-item"));
                renderer.WriteChildren(listItemBlock);
                renderer.Pop();
            }

            renderer.Pop();
        }
    }
}

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
    using static Helpers.VisualElementUtility;
    using static Helpers.UnityPathUtility;
    public class ListRenderer : UIElementObjectRenderer<ListBlock>
    {
        protected override void Write(UIElementRenderer renderer, ListBlock listBlock)
        {
            renderer.Push(GetClassedElement<VisualElement>("list"));

            //if (listBlock.IsOrdered)
            //{
            //    list.MarkerStyle = TextMarkerStyle.Decimal;

            //    if (listBlock.OrderedStart != null && (listBlock.DefaultOrderedStart != listBlock.OrderedStart))
            //    {
            //        list.StartIndex = int.Parse(listBlock.OrderedStart, NumberFormatInfo.InvariantInfo);
            //    }
            //}
            //else
            //{
            //    list.MarkerStyle = TextMarkerStyle.Disc;
            //}

            foreach (var item in listBlock.OfType<ListItemBlock>())
            {
                var listItem = GetClassedElement<VisualElement>("list-item");
                renderer.Push(listItem);
                renderer.WriteChildren(item);
                renderer.Pop();
            }

            renderer.Pop();
        }
    }
}

//using ColorCode;
//using ColorCode.Common;
//using ColorCode.Parsing;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//#if UNITY_2019_1_OR_NEWER
//using UnityEngine.UIElements;
//#else
//using UnityEngine.Experimental.UIElements;
//#endif
//using Color = ColorCode.Styling.Color;
//using UColor = UnityEngine.Color;

//namespace ThunderKit.Markdown.SyntaxHighlighting
//{
//    using static Helpers.VisualElementFactory;
//    public class UIElementsFormatter : IFormatter
//    {
//        public UIElementRenderer renderer;
//        static readonly char[] splitChars = new char[] { '\r' };
//        public void Write(string parsedSourceCode, IList<Scope> scopes, IStyleSheet styleSheet, TextWriter textWriter)
//        {
//            if (parsedSourceCode == "" && parsedSourceCode == " ") return;
//            if (parsedSourceCode == "\r\n") return;
//            var parts = parsedSourceCode.Replace("\n", "\r").Split(splitChars, System.StringSplitOptions.RemoveEmptyEntries);
//            if (parts.Length == 1 && parts[0] != " ") renderer.WriteInline(GetTextElement<Label>(parsedSourceCode, "inline"));
//            else
//                foreach (var part in parts)
//                {
//                    var trimmedPart = part.Trim(' ');
//                    switch (trimmedPart)
//                    {
//                        case "":
//                        case " ":
//                            break;
//                        default:
//                            renderer.Push(GetClassedElement<VisualElement>("code-line"));
//                            renderer.WriteInline(GetTextElement<Label>(trimmedPart, "inline"));
//                            renderer.Pop();
//                            break;
//                    }
//                }
//        }

//        //if (Result == null) return;

//        //if (scopes.Count == 0)
//        //    Result.Add(GetTextElement<Label>(parsedSourceCode, "inline"));
//        //else
//        //    foreach (var scope in scopes)
//        //    {
//        //        var label = GetClassedElement<Label>("inline");
//        //        var styles = styleSheet.Styles;
//        //        var scopeName = scope?.Name;
//        //        if (styles.Contains(scopeName))
//        //        {
//        //            Style style = styleSheet.Styles[scope.Name];
//        //            if (style.Italic) label.AddToClassList("italic");
//        //            if (style.Bold) label.AddToClassList("bold");
//        //            if (style.Background != null)
//        //            {
//        //                var bg = style.Background.ToHtmlColor();
//        //                if (ColorUtility.TryParseHtmlString(bg, out UColor bgColor))
//        //                    label.style.backgroundColor = bgColor;
//        //            }
//        //            if (style.Foreground != null)
//        //            {
//        //                var fg = style.Foreground.ToHtmlColor();
//        //                if (ColorUtility.TryParseHtmlString(fg, out UColor fgColor))
//        //                    label.style.color = fgColor;
//        //            }
//        //        }
//        //        label.text = parsedSourceCode;
//        //        Result.Add(label);
//        //    }
//        public void WriteFooter(IStyleSheet styleSheet, ILanguage language, TextWriter textWriter)
//        {
//        }
//        public void WriteHeader(IStyleSheet styleSheet, ILanguage language, TextWriter textWriter)
//        {
//        }

//    }
//}
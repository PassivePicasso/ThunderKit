using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif


namespace ThunderKit.Core.UIElements
{
    public class SlightlyRichTextElement : VisualElement
    {
        public string Text { get; set; }

        public SlightlyRichTextElement()
        {
            style.flexDirection  = FlexDirection.Row;
            style.flexWrap = Wrap.Wrap;
        }
        public new class UxmlFactory : UxmlFactory<SlightlyRichTextElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription m_text = new UxmlStringAttributeDescription { name = "text" };
            // Regex for maybe providing options or settings?
            //\[(.*?)\]\((.*?),?(\w+\:\w,?)*\)|([\S]+\b\S?)|\s+

            Regex IntegratedExpression = new Regex("^\\[(.*?)\\]\\<(\\w*?)\\>\\((.*?)\\)|^([\\S]+\\b\\S?)|^\\s+", RegexOptions.Singleline | RegexOptions.Compiled);

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var slightlyRichTextElement = (SlightlyRichTextElement)ve;
                slightlyRichTextElement.Text = m_text.GetValueFromBag(bag, cc);
                var run = slightlyRichTextElement.Text;
                slightlyRichTextElement.Clear();

                int safetyBreak = 0;
                for (int i = 0; i < run.Length;)
                {
                    var value = string.Empty;
                    var match = IntegratedExpression.Match(run);
                    if (match.Success)
                    {
                        if (string.IsNullOrEmpty(match.Value) || string.IsNullOrWhiteSpace(match.Value))
                        {
                            value = match.Value;
                            safetyBreak = 0;
                        }
                        else if (match.Groups[1].Success && match.Groups[2].Success && match.Groups[3].Success)
                        { }
                        else if (match.Groups[3].Success && match.Groups[4].Success && match.Groups[3].Success)
                        {
                            value = match.Value;
                            switch (match.Groups[3].Value)
                            {
                                case "AssetLink":
                                    ve.Add(new AssetLink
                                    {
                                        text = match.Groups[1].Value,
                                        AssetPath = match.Groups[3].Value
                                    });
                                    break;
                                //case "DocLink":
                                //    ve.Add(new DocLink
                                //    {
                                //        text = match.Groups[1].Value,
                                //        AssetPath = match.Groups[3].Value
                                //    });
                                //    break;
                            }
                            safetyBreak = 0;
                        }
                        else if (match.Groups[4].Success)
                        {
                            ve.Add(new Label { text = value = match.Groups[4].Value });
                            safetyBreak = 0;
                        }
                        run = run.Substring(value.Length);
                    }
                    else
                    {
                        Debug.LogError("Parse failed");
                        break;
                    }

                    if (++safetyBreak > 10 || run.Length == 0 || string.IsNullOrWhiteSpace(run))
                        break;
                }
            }


            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }

    }
}

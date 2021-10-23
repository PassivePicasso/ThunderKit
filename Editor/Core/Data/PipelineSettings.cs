using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using ThunderKit.Common;
using ThunderKit.Core.Config;
using ThunderKit.Markdown;
using ThunderKit.Core.Windows;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Data
{
    public class PipelineSettings : ThunderKitSetting
    {
        const string DocumentationStylePath = "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss";
        public string TimeFormat = "HH:mm:ss:fff";
        public string DateFormat = "dd-MM-yyyy";

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            var helpInfo = new MarkdownElement
            {
                Data = "Refer to [Date Time Format](documentation://Packages/com.passivepicasso.thunderkit/Documentation/topics/DateTimeFormat.uxml) for formatting information",
                MarkdownDataType = MarkdownDataType.Text
            };
#if UNITY_2018
            helpInfo.AddStyleSheetPath(DocumentationStylePath);
#else
            helpInfo.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(DocumentationStylePath));
#endif
            helpInfo.RefreshContent();
            helpInfo.AddToClassList("m4");
            rootElement.Add(helpInfo);
            rootElement.Add(CreateStandardField(nameof(TimeFormat)));
            rootElement.Add(CreateStandardField(nameof(DateFormat)));
            rootElement.Bind(new SerializedObject(this));
        }
    }
}
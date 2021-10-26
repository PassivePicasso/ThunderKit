using UnityEditor;
using ThunderKit.Markdown;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Data
{
    public class PipelineLogSettings : ThunderKitSetting
    {
        const string DocumentationStylePath = "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss";
        public string DateTimeFormat = "HH:mm:ss:fff";

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
            rootElement.Add(CreateStandardField(nameof(DateTimeFormat)));
            rootElement.Bind(new SerializedObject(this));
        }
    }
}
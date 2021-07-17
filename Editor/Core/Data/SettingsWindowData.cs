using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;
using ThunderKit.Markdown;
using ThunderKit.Core.Windows;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif


namespace ThunderKit.Editor.Core.Data
{
    public class SettingsWindowData : ThunderKitSetting
    {
        [InitializeOnLoadMethod]
        static void SettingsWindowSetup()
        {
            EditorApplication.wantsToQuit -= EditorApplication_wantsToQuit;
            EditorApplication.wantsToQuit += EditorApplication_wantsToQuit;

            var settings = GetOrCreateSettings<SettingsWindowData>();
            if (settings.FirstLoad && settings.ShowOnStartup)
                EditorApplication.update += ShowSettings;
        }

        private static void ShowSettings()
        {
            EditorApplication.update -= ShowSettings;
            Settings.ShowSettings();
            var settings = GetOrCreateSettings<SettingsWindowData>();
            settings.FirstLoad = false;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private static bool EditorApplication_wantsToQuit()
        {
            var settings = GetOrCreateSettings<SettingsWindowData>();
            settings.FirstLoad = true;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return true;
        }

        [SerializeField]
        private bool FirstLoad = true;

        public bool ShowOnStartup = true;
        private SerializedObject settingsWindowDataSo;

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            MarkdownElement markdown = new MarkdownElement
            {
                Data = $@"Welcome and Thank you for trying ThunderKit.  Please configure your ThunderKit project by first clicking the Locate Game button below!

If this is your first time using ThunderKit, [Click Here](menulink://Tools/ThunderKit/Documentation) to launch the documentation",
                MarkdownDataType = MarkdownDataType.Text
            };
#if UNITY_2018
            markdown.AddStyleSheetPath("Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss");
            markdown.AddStyleSheetPath("Packages/com.passivepicasso.thunderkit/Documentation/uss/thunderkit_documentation.uss");
#else
            markdown.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss"));
            markdown.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.passivepicasso.thunderkit/Documentation/uss/thunderkit_documentation.uss"));
#endif
            markdown.RefreshContent();
            markdown.AddToClassList("m4");

            var child = CreateStandardField(nameof(ShowOnStartup));
            child.tooltip = "Uncheck this to stop showing this window on startup";
            rootElement.Add(child);
            rootElement.Add(markdown);

            if (settingsWindowDataSo == null)
                settingsWindowDataSo = new SerializedObject(this);

            rootElement.Bind(settingsWindowDataSo);
        }
    }
}
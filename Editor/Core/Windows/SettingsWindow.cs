using System.Linq;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Windows;
using UnityEditor;
using System;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Windows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;
    public class SettingsWindow : TemplatedWindow
    {
        readonly static string[] searchFolders = new[] { "Assets", "Packages" };

        public static event Action OnSettingsLoading;

        [MenuItem(Constants.ThunderKitMenuRoot + "Settings")]
        public static void ShowSettings()
        {
            OnSettingsLoading?.Invoke();
            GetWindow<SettingsWindow>();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            var settingsArea = rootVisualElement.Q("settings-area");
            var settingsPaths = AssetDatabase.FindAssets($"t:{nameof(ThunderKitSetting)}", searchFolders)
                .Select(AssetDatabase.GUIDToAssetPath).ToArray();

            foreach (var settingPath in settingsPaths)
            {
                var setting = AssetDatabase.LoadAssetAtPath<ThunderKitSetting>(settingPath);
                if (!setting)
                {
                    AssetDatabase.DeleteAsset(settingPath);
                    continue;
                }
                var settingSection = GetTemplateInstance("ThunderKitSettingSection");
                var title = settingSection.Q<Label>("title");
                if (title != null)
                    title.text = setting.name;
                var properties = settingSection.Q<VisualElement>("properties");
                try
                {
                    setting.CreateSettingsUI(properties);
                }
                catch
                {
                    var errorLabel = new Label($"Failed to load settings user interface");
                    errorLabel.AddToClassList("thunderkit-error");
                    properties.Add(errorLabel);
                }
                settingsArea.Add(settingSection);
            }
        }
    }
}
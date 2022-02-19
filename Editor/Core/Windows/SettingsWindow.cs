using System.Linq;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Windows;
using UnityEditor;
using System;
using UnityEngine;
using System.Collections.Generic;
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
        private ListView settingsList;
        private VisualElement settingsArea;

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

            settingsList = rootVisualElement.Q("settings-list") as ListView;
            settingsArea = rootVisualElement.Q("settings-area");
            var settings = AssetDatabase.FindAssets($"t:{nameof(ThunderKitSetting)}", Constants.FindAllFolders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ThunderKitSetting>)
                .ToArray();

            settingsList.makeItem = OnMakeItem;
            settingsList.bindItem = OnBindItem; 
            settingsList.itemsSource = settings;
            settingsList.selectionType = SelectionType.Multiple;

#if UNITY_2020_2_OR_NEWER
            settingsList.onSelectionChange += OnSettingsSelected;
#else
            settingsList.onSelectionChanged += OnSettingsSelected;
#endif
        }

        private void OnBindItem(VisualElement ve, int index)
        {
            var label = ve as Label;
            var setting = settingsList.itemsSource[index] as ThunderKitSetting;
            label.text = setting.DisplayName;
        }

        private VisualElement OnMakeItem() => new Label();

        void OnSettingsSelected(IEnumerable<object> obj)
        {
            settingsArea.Clear();
            foreach (var setting in obj.OfType<ThunderKitSetting>())
            {
                var settingSection = GetTemplateInstance("ThunderKitSettingSection");
                var title = settingSection.Q<Label>("title");
                if (title != null)
                    title.text = setting.DisplayName;
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
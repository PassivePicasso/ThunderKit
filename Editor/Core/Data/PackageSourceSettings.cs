using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using ThunderKit.Core.UIElements;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Data
{
    // Create a new type of Settings Asset.
    public class PackageSourceSettings : ThunderKitSetting
    {
        const string SettingsTemplatesPath = "Packages/com.passivepicasso.thunderkit/Editor/Core/Templates/Settings";
        const string PackageSourceSettingsTemplatePath = SettingsTemplatesPath + "/PackageSourceSettings.uxml";

        public PackageSource[] PackageSources => AssetDatabase.FindAssets($"t:{nameof(PackageSource)}")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<PackageSource>(path))
                .ToArray();

        private ListView sourceList;
        private Button addSourceButton;
        private Button removeSourceButton;
        private Button refreshButton;
        private ScrollView selectedSourceSettings;

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            var settingsElement = TemplateHelpers.LoadTemplateInstance(PackageSourceSettingsTemplatePath);
            selectedSourceSettings = settingsElement.Q<ScrollView>("selected-source-settings");
            sourceList = settingsElement.Q<ListView>("sources-list");
            addSourceButton = settingsElement.Q<Button>("add-source-button");
            removeSourceButton = settingsElement.Q<Button>("remove-source-button");
            refreshButton = settingsElement.Q<Button>("refresh-button");

            if (removeSourceButton != null)
            {
                removeSourceButton.clickable.clicked += RemoveSourceClicked;
            }
            refreshButton.clickable.clicked += Refresh;

            addSourceButton.clickable.clicked += OpenAddSourceMenu;

            sourceList.selectionType = SelectionType.Multiple;
            sourceList.makeItem = () => new Label() { name = "source-name-item" };
            sourceList.bindItem = (ve, i) =>
            {
                var label = ve as Label;
                if (label != null)
                {
                    label.text = PackageSources[i].name;
                    //label.tooltip = $"Type: {PackageSources[i].Name}\r\nGroup: {PackageSources[i].SourceGroup}";
                }
            };
            sourceList.itemsSource = PackageSources;
#if UNITY_2020_2_OR_NEWER
            sourceList.onSelectionChange += OnSelectionChanged;
#else
            sourceList.onSelectionChanged += OnSelectionChanged;
#endif
            rootElement.Add(settingsElement);
        }

        private void OnSourceNameBlur(BlurEvent evt)
        {
            var sourceName = evt.currentTarget as TextField;
            var source = sourceName.userData as PackageSource;
            string path = AssetDatabase.GetAssetPath(source);
            var result = AssetDatabase.RenameAsset(path, sourceName.text);
#if UNITY_2021_2_OR_NEWER
            sourceList.Rebuild();
#else
            sourceList.Refresh();
#endif
            if (!string.IsNullOrEmpty(result))
                Debug.LogError(result);
        }
        private void OnSelectionChanged(IEnumerable<object> sources)
        {
            if (removeSourceButton == null || sources == null) return;
            removeSourceButton.userData = sources;
            selectedSourceSettings.Clear();
            foreach (var source in sources.OfType<PackageSource>())
            {
                try
                {
                    var settingsInstance = TemplateHelpers.LoadTemplateInstance($"{SettingsTemplatesPath}/{source.GetType().Name}.uxml");
                    selectedSourceSettings.Add(settingsInstance);
                    var nameField = settingsInstance.Q<TextField>("asset-name-field");
                    if (nameField != null)
                    {
                        nameField.userData = source;
                        nameField.RegisterCallback<BlurEvent>(OnSourceNameBlur, TrickleDown.NoTrickleDown);
                        nameField.value = source.name;
                    }
                    settingsInstance.Bind(new SerializedObject(source));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
#if UNITY_2019_1_OR_NEWER
            selectedSourceSettings.contentContainer.StretchToParentWidth();
#elif UNITY_2018_1_OR_NEWER
            selectedSourceSettings.stretchContentWidth = true;
#endif
        }

        private void RemoveSourceClicked()
        {
            var source = removeSourceButton.userData as List<object>;
            if (source == null) return;
            var updatedPackageSources = PackageSources.Where(s => !source.Contains(s)).ToArray();
            bool refresh = false;
            for (int i = 0; i < PackageSources.Length; i++)
                if (source.Contains(PackageSources[i]))
                {
                    refresh = true;
                    string path = AssetDatabase.GetAssetPath(PackageSources[i]);
                    DestroyImmediate(PackageSources[i], true);
                    AssetDatabase.DeleteAsset(path);
                }
            if (refresh)
                RefreshList();
            removeSourceButton.userData = null;
        }

        private void OnNameChanged(ChangeEvent<string> evt)
        {
            var sourceName = evt.currentTarget as TextField;
            var source = sourceName.userData as PackageSource;
            source.name = evt.newValue;
        }

        private void OpenAddSourceMenu()
        {
            var sourceTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(asm =>
                    {
                        try { return asm.GetTypes(); }
                        catch { return Array.Empty<Type>(); }
                    })
                    .Where(t => typeof(PackageSource).IsAssignableFrom(t) && t != typeof(PackageSource) && !t.IsAbstract)
                    .ToArray();

            var menu = new GenericMenu();

            foreach (var type in sourceTypes)
                menu.AddItem(
                    new GUIContent($"{type.Name}"),
                    false,
                    () =>
                    {
                        const string SettingsPath = "Assets/ThunderKitSettings";
                        var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{SettingsPath}/{type.Name}.asset");
                        ScriptableHelper.EnsureAsset(assetPath, type);
                        RefreshList();
                    }
                );

            menu.ShowAsContext();
        }

        private void Refresh()
        {
            PackageSource.LoadAllSources();
        }

        private void RefreshList()
        {
            if (sourceList != null)
            {
                sourceList.itemsSource = PackageSources;
#if UNITY_2021_2_OR_NEWER
                sourceList.Rebuild();
#else
                sourceList.Refresh();
#endif
            }
        }
        readonly string[] keywords = new string[] { };
        public override IEnumerable<string> Keywords() => keywords;
    }
}
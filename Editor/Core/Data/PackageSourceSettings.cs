﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using ThunderKit.Core.UIElements;
using ThunderKit.Common;
using ThunderKit.Core.Utilities;
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
        public static List<PackageSource> PackageSources { get; private set; } = new List<PackageSource>();
        static Queue<(PackageSource source, bool add)> PendingPackageSourceChanges { get; set; } = new Queue<(PackageSource source, bool add)>();

        private ListView sourceList;
        private Button addSourceButton;
        private Button removeSourceButton;
        private Button refreshButton;
        private ScrollView selectedSourceSettings;

        private static readonly string ThunderKitSettingsFolder = "Assets/ThunderKitSettings";
        private static string[] PackageSourceFolder = new string[] { ThunderKitSettingsFolder };

        [InitializeOnLoadMethod]
        public static void InitSources()
        {
            EditorApplication.projectChanged += RefreshSources;
            RefreshSources();
        }

        private static void RefreshSources()
        {
            PackageSources?.Clear();
            if (PackageSources == null)
                PackageSources = new List<PackageSource>();
            var assetsFolders = AssetDatabase.GetSubFolders("Assets");

            if (assetsFolders.Contains("Assets/ThunderKitSettings"))
            {
                var assetGuids = AssetDatabase.FindAssets($"t:{nameof(PackageSource)}", PackageSourceFolder);
                var assetPaths = assetGuids.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
                foreach (var asset in assetPaths.Select(path => AssetDatabase.LoadAssetAtPath<PackageSource>(path)))
                    RegisterSource(asset);
            }
        }

        private static void ProcessRegistrations()
        {
            for (int i = PackageSources.Count - 1; i > -1; i--)
            {
                if (!PackageSources[i])
                {
                    PackageSources.RemoveAt(i);
                }
            }
        }

        public static void RegisterSource(PackageSource source)
        {
            PendingPackageSourceChanges.Enqueue((source, true));
            EditorApplication.update += DeferredRefresh;
        }

        public static void UnregisterSource(PackageSource source)
        {
            PendingPackageSourceChanges.Enqueue((source, false));
            EditorApplication.update += DeferredRefresh;
        }

        private static void DeferredRefresh()
        {
            while (PendingPackageSourceChanges.Any())
            {
                var pendingChange = PendingPackageSourceChanges.Dequeue();
                if (!pendingChange.source) continue;
                if (EditorUtility.IsPersistent(pendingChange.source))
                {
                    if (pendingChange.add)
                    {
                        if (!PackageSources.Contains(pendingChange.source))
                            PackageSources.Add(pendingChange.source);
                    }
                    else
                        PackageSources.Remove(pendingChange.source);
                }
            }
            ProcessRegistrations();
            EditorApplication.update -= DeferredRefresh;
            EditorApplication.update += GetOrCreateSettings<PackageSourceSettings>().RefreshList;
        }

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            var settingsElement = TemplateHelpers.LoadTemplateInstance(Constants.PackageSourceSettingsTemplatePath);
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
#if UNITY_2020_1_OR_NEWER
            sourceList.onSelectionChange += OnSelectionChanged;
#else
            sourceList.onSelectionChanged += OnSelectionChanged;
#endif
            RefreshList();
            rootElement.Add(settingsElement);
        }

        private void OnSourceNameBlur(BlurEvent evt)
        {
            var sourceName = evt.currentTarget as TextField;
            var source = sourceName.userData as PackageSource;
            string path = AssetDatabase.GetAssetPath(source);
            var result = AssetDatabase.RenameAsset(path, sourceName.text);
            RefreshList();

            if (!string.IsNullOrEmpty(result))
                Debug.LogError(result);
        }

        private void OnSelectionChanged(IEnumerable<object> sources)
        {
            if (removeSourceButton == null || sources == null)
                return;
            selectedSourceSettings.Clear();
            var selectedSources = sources.OfType<PackageSource>().ToList();
            foreach (var source in selectedSources)
            {
                try
                {
                    var settingsInstance = TemplateHelpers.LoadTemplateInstance($"{Constants.SettingsTemplatesPath}/{source.GetType().Name}.uxml");
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

        public static void RemoveSource(PackageSource source)
        {
            string path = AssetDatabase.GetAssetPath(source);
            DestroyImmediate(source, true);
            AssetDatabase.DeleteAsset(path);
        }

        public void RemoveSourceClicked()
        {
            if (sourceList.selectedItem == null)
                return;

            PackageSource sourceToDelete = (PackageSource)sourceList.selectedItem;
            if (PackageSources.Contains(sourceList.selectedItem) == false)
            {
                Debug.LogWarning($"Selected item {sourceList.selectedItem} is not a valid PackageSource.");
                return;
            }
            PackageSourceSettings.RemoveSource(sourceToDelete);

            Refresh();
            EditorApplication.update -= RefreshList;
            EditorApplication.update += RefreshList;
        }

        private void OnNameChanged(ChangeEvent<string> evt)
        {
            var sourceName = evt.currentTarget as TextField;
            var source = sourceName.userData as PackageSource;
            source.name = evt.newValue;
        }

        public static Type[] GetAvailablePackageSourceTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm =>
            {
                try { return asm.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .Where(t => typeof(PackageSource).IsAssignableFrom(t) && t != typeof(PackageSource) && !t.IsAbstract)
            .ToArray();
        }

        public void AddSource(Type packageSourceType)
        {
            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{ThunderKitSettingsFolder}/{packageSourceType.Name}.asset");
            ScriptableHelper.EnsureAsset(assetPath, packageSourceType);
            EditorApplication.update -= RefreshList;
            EditorApplication.update += RefreshList;
        }

        private void OpenAddSourceMenu()
        {
            var menu = new GenericMenu();
            var packageSourceTypes = PackageSourceSettings.GetAvailablePackageSourceTypes();
            foreach (var type in packageSourceTypes)
                menu.AddItem(
                    new GUIContent($"{type.Name}"),
                    false,
                    () =>
                    AddSource(type)
                );

            menu.ShowAsContext();
        }

        public void Refresh()
        {
            PackageSource.LoadAllSources();
        }

        private void RefreshList()
        {
            EditorApplication.update -= RefreshList;
            if (sourceList != null)
            {
                sourceList.itemsSource = PackageSources.ToList();
#if UNITY_2021_2_OR_NEWER
                sourceList.ClearSelection();
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
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Actions;
using UnityEditor;
using UnityEngine;
using PackageSource = ThunderKit.Core.Data.PackageSource;
using System;
using ThunderKit.Common.Configuration;
using ThunderKit.Core.Utilities;
using ThunderKit.Core.UIElements;
using ThunderKit.Markdown;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Windows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;
    public class PackageManager : TemplatedWindow
    {
        private static readonly PackageVersion[] EmptyPackages = new PackageVersion[0];

        private VisualElement packageView;
        private Button filtersButton, refreshButton;
        private TextField searchBox;

        private string targetVersion;
        private readonly Dictionary<string, bool> tagEnabled = new Dictionary<string, bool>();

        [SerializeField] private DeletePackage deletePackage;
        [SerializeField] private string SearchString;

        public bool InProject;

        [MenuItem(Constants.ThunderKitMenuRoot + "Packages")]
        public static void ShowExample() => GetWindow<PackageManager>();

        public override void OnEnable()
        {
            PackageSource.SourcesInitialized -= PackageSource_SourceInitialized;
            PackageSource.SourcesInitialized += PackageSource_SourceInitialized;
            EditorApplication.update += OnLoad;
            if (rootVisualElement == null)
            {
                PackageSource.SourcesInitialized -= PackageSource_SourceInitialized;
                return;
            }

            titleContent = new GUIContent("Packages", ThunderKitIcon, "");
            rootVisualElement.Clear();

            GetTemplateInstance("PackageManagerData", rootVisualElement);

            packageView = rootVisualElement.Q("tkpm-package-view");
            searchBox = rootVisualElement.Q<TextField>("tkpm-search-textfield");
            //searchBoxCancel = rootVisualElement.Q<Button>("tkpm-search-cancelbutton");
            filtersButton = rootVisualElement.Q<Button>("tkpm-filters-selector");
            refreshButton = rootVisualElement.Q<Button>("tkpm-refresh-button");

            searchBox.RegisterCallback<ChangeEvent<string>>(OnSearchText);
            searchBox.SetValueWithoutNotify(SearchString);

            filtersButton.clickable.clicked -= FiltersClicked;
            filtersButton.clickable.clicked += FiltersClicked;

            refreshButton.clickable.clicked -= RefreshClicked;
            refreshButton.clickable.clicked += RefreshClicked;

            GetTemplateInstance("PackageView", packageView);
            ConstructPackageSourceList(PackageSourceSettings.PackageSources);
        }

        private void OnLoad()
        {
            EditorApplication.update -= OnLoad;
            PackageSource.LoadAllSources();
        }

        private void OnInspectorUpdate()
        {
            TryDelete();
        }

        private void OnDestroy()
        {
            PackageSource.SourcesInitialized -= PackageSource_SourceInitialized;
        }

        private void PackageSource_SourceInitialized(object sender, EventArgs e)
        {
            ConstructPackageSourceList(PackageSourceSettings.PackageSources);
        }

        private void RefreshClicked()
        {
            PackageSource.LoadAllSources();
        }

        private void ConstructPackageSourceList(List<PackageSource> packageSources)
        {
            var packageSourceList = rootVisualElement.Q(name = "tkpm-package-source-list");

            var existingSources = new List<PackageSource>();
            foreach (var child in packageSourceList.Children().ToArray())
            {
                var source = child.userData as PackageSource;
                if (!packageSources.Contains(source))
                    child.RemoveFromHierarchy();
                existingSources.Add(source);
                var groupName = $"tkpm-package-source-{NormalizeName(source.name)}";
                child.name = groupName;
            }
            foreach (var packageSource in packageSources.Except(existingSources))
            {
                var groupName = $"tkpm-package-source-{NormalizeName(packageSource.name)}";
                var packageSourceView = GetTemplateInstance("PackageSource");
                var packageList = packageSourceView.Q<ListView>("tkpm-package-list");
                var foldOut = packageSourceView.Q<Foldout>();
                packageSourceView.RemoveFromClassList("grow");
                foldOut.value = false;
                foldOut.RegisterCallback<ChangeEvent<bool>>((evt) =>
                {
                    if (evt.newValue)
                        packageSourceView.AddToClassList("grow");
                    else
                        packageSourceView.RemoveFromClassList("grow");
                });
                var loadingIndicator = packageSourceView.Q<LoadingSpinner>(name: "tkpm-package-source-loading-indicator");
                packageSource.OnLoadingStarted += () => loadingIndicator.Start();
                packageSource.OnLoadingStopped += () => loadingIndicator.Stop();

                packageSourceView.AddToClassList("tkpm-package-source");
                packageSourceView.name = groupName;
                packageSourceView.userData = packageSource;

                packageList.selectionType = SelectionType.Single;

#if UNITY_2020_1_OR_NEWER
                packageList.onSelectionChange -= PackageList_onSelectionChanged;
                packageList.onSelectionChange += PackageList_onSelectionChanged;
#else
                packageList.onSelectionChanged -= PackageList_onSelectionChanged;
                packageList.onSelectionChanged += PackageList_onSelectionChanged;
#endif

                packageList.makeItem = () =>
                {
                    var packageInstance = GetTemplateInstance("Package");
                    packageInstance.userData = packageList;
                    packageInstance.AddToClassList("tkpm-package-option");
                    return packageInstance;
                };
                packageList.bindItem = BindPackageListViewItem;

                packageSourceList.Add(packageSourceView);
            }
            UpdatePackageList();
        }

        private void FiltersClicked()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(nameof(InProject))), InProject, () =>
            {
                InProject = !InProject;
                UpdatePackageList();
            });
            foreach (var tag in tagEnabled.Keys)
            {
                menu.AddItem(new GUIContent($"Tags/{tag}"), tagEnabled[tag], () =>
                {
                    tagEnabled[tag] = !tagEnabled[tag];
                    UpdatePackageList();
                });
            }
            menu.ShowAsContext();
        }

        private void OnSearchText(ChangeEvent<string> evt)
        {
            SearchString = evt.newValue;
            UpdatePackageList();
        }

        string NormalizeName(string name) => name.Replace(" ", "-").ToLower();

        void UpdatePackageList()
        {
            for (int sourceIndex = 0; sourceIndex < PackageSourceSettings.PackageSources.Count; sourceIndex++)
            {
                var source = PackageSourceSettings.PackageSources[sourceIndex];
                var packageSource = rootVisualElement.Q($"tkpm-package-source-{NormalizeName(source.name)}");
                var headerLabel = packageSource.Q<Label>("tkpm-package-source-label");
                var packageList = packageSource.Q<ListView>("tkpm-package-list");

                packageList.itemsSource = FilterPackages(source.Packages);

#if UNITY_2021_2_OR_NEWER
                packageList.Rebuild();
#else
                packageList.Refresh();
#endif

                if (sourceIndex == 0 && packageList.itemsSource.Count > 0)
                {
                    packageList.selectedIndex = 0;
                    BindPackageView(packageList.selectedItem as PackageGroup);
                }

                var pkgs = source.Packages.Where(pkg => pkg);
                var allTags = pkgs?.Where(pkg => pkg.Tags != null).SelectMany(pkg => pkg.Tags);
                var distinctTags = allTags?.Distinct();
                var pathedTags = distinctTags?.Select(tag => $"{source?.Name}/{tag}");
                var tags = (pathedTags ?? Enumerable.Empty<string>());
                foreach (var tag in tags)
                    if (tagEnabled.ContainsKey(tag)) tagEnabled[tag] = tagEnabled[tag];
                    else
                        tagEnabled[tag] = false;

                var headerText = $"{source.name} ({packageList.itemsSource.Count})";
                var hiddenCount = source.Packages.Count - packageList.itemsSource.Count;
                if (hiddenCount > 0)
                    headerText = $"{headerText} {hiddenCount} hidden";

                headerLabel.text = headerText;
            }
        }

        List<PackageGroup> FilterPackages(List<PackageGroup> packages)
        {
            var enabledTags = tagEnabled.Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key.Substring(kvp.Key.LastIndexOf('/') + 1))
                .ToArray();

            packages = packages.Where(pkg => pkg).ToList();

            var hasTags = enabledTags.Any() ? packages.Where(pkg => pkg.Tags != null).Where(pkg => enabledTags.All(tag => ArrayUtility.Contains(pkg.Tags, tag))) : packages;
            if (!hasTags.Any())
                return hasTags.ToList();

            var hasString = hasTags.Where(pkg => pkg.HasString(SearchString ?? string.Empty));
            if (!hasString.Any())
                return hasString.ToList();

            var filteredByInstall = !InProject ? hasString : hasString.Where(pkg => pkg.Installed);

            return filteredByInstall.ToList();
        }

        void BindPackageListViewItem(VisualElement packageElement, int packageIndex)
        {
            var sourceList = packageElement.userData as ListView;
            var package = sourceList.itemsSource[packageIndex] as PackageGroup;
            packageElement.name = $"tkpm-package-{package.PackageName}-{package["latest"].version}";

            var packageInstalled = packageElement.Q<Image>("tkpm-package-installed");

            if (package.Installed) packageInstalled.AddToClassList("installed");
            else
                packageInstalled.RemoveFromClassList("installed");

            var packageName = packageElement.Q<Label>("tkpm-package-name");
            if (packageName != null)
                packageName.tooltip =
                   packageName.text = NicifyPackageName(package.PackageName);

            var packageVersion = packageElement.Q<Label>("tkpm-package-version");
            if (packageVersion != null) packageVersion.text = package["latest"].version;
        }

        private void PackageList_onSelectionChanged(IEnumerable<object> obj)
        {
            var selection = obj.OfType<PackageGroup>().FirstOrDefault();
            if (selection == null) return;
            BindPackageView(selection);
        }

        private void BindPackageView(PackageGroup selection)
        {
            var latest = selection["latest"];
            UpdateTargetVersion(selection, latest);
            var viewheader = packageView.Q("tkpm-package-view-header");
            var versionButton = viewheader.Q<Button>(name: "tkpm-package-version-button");
            BindVersionButton(selection, versionButton);
            BindInstallButton(selection);

            BindMarkdownElement("tkpm-package-title", selection.HeaderMarkdown);
            BindMarkdownElement("tkpm-package-details", latest.VersionMarkdown);
            BindMarkdownElement("tkpm-package-footer-markdown", selection.FooterMarkdown);

            var dependencies = packageView.Q("tkpm-package-dependencies");
            var tags = packageView.Q("tkpm-package-tags");

            BindLabels(dependencies.parent.parent, dependencies, GetDependencies(selection), "dependency");
            BindLabels(tags.parent, tags, selection.Tags, "tag");

            BindLabel(packageView, "tkpm-package-name", selection.DependencyId);
            if (selection.Installed)
                BindLabel(packageView, "tkpm-package-info-version-value", selection.InstalledVersion);
            else
                BindLabel(packageView, "tkpm-package-info-version-value", latest.version);

            BindLabel(packageView, "tkpm-package-author-value", selection.Author);
        }

        private IEnumerable<string> GetDependencies(PackageGroup selection)
        {
            var selectedVersion = selection[targetVersion];
            var pvDependencies = selectedVersion?.dependencies ?? EmptyPackages;
            var dependencyIds = pvDependencies.Where(pv => pv != null).Select(pvd => pvd.dependencyId).ToList();
            var dependencies = dependencyIds ?? Enumerable.Empty<string>();
            return dependencies;
        }

        private void UpdateTargetVersion(PackageGroup selection, PackageVersion latest)
        {
            if (selection.Installed)
                targetVersion = PackageHelper.GetPackageManagerManifest(selection.InstallDirectory).version;
            else
                targetVersion = latest.version;
        }

        private void BindMarkdownElement(string elementName, string headerMarkdown)
        {
            var markdownElement = packageView.Q<MarkdownElement>(elementName);
            markdownElement.Data = headerMarkdown;
            if (!string.IsNullOrWhiteSpace(markdownElement.Data))
            {
#if UNITY_2019_1_OR_NEWER
                markdownElement.style.display = DisplayStyle.Flex;
#else
                markdownElement.style.visibility = Visibility.Visible;
#endif
                markdownElement.RefreshContent();
            }
            else
            {
#if UNITY_2019_1_OR_NEWER
                markdownElement.style.display = DisplayStyle.None;
#else
                markdownElement.style.visibility = Visibility.Hidden;
#endif
                markdownElement.RefreshContent();
            }
        }

        void BindLabel(VisualElement root, string name, string text)
        {
            var label = root.Q<Label>(name);
            if (label != null) label.text = text;
        }

        void BindLabels(VisualElement hidingElement, VisualElement labelContainer, IEnumerable<string> texts, params string[] classes)
        {
            labelContainer.Clear();
            if (texts == null || !texts.Any())
            {
                labelContainer.parent.AddToClassList("hidden");
#if UNITY_2019_1_OR_NEWER
                hidingElement.style.display = DisplayStyle.None;
#else
                hidingElement.style.visibility = Visibility.Hidden;
#endif
                return;
            }
            else
            {
                labelContainer.parent.RemoveFromClassList("hidden");
#if UNITY_2019_1_OR_NEWER
                hidingElement.style.display = DisplayStyle.Flex;
#else
                hidingElement.style.visibility = Visibility.Visible;
#endif
            }

            foreach (var text in texts)
            {
                if (string.IsNullOrEmpty(text)) continue;
                var label = new Label(text);
                foreach (var clss in classes)
                    if (!string.IsNullOrEmpty(clss))
                        label.AddToClassList(clss);
                labelContainer.Add(label);
            }
        }

        #region Installation
        void BindInstallButton(PackageGroup selection)
        {
            var installButton = packageView.Q<Button>("tkpm-package-install-button");
            installButton.userData = selection;
            installButton.clickable.clickedWithEventInfo -= UpdateInstallation;
            installButton.clickable.clickedWithEventInfo += UpdateInstallation;
            installButton.text = selection.Installed ? "Uninstall" : "Install";
        }

        async void UpdateInstallation(EventBase obj)
        {
            try
            {
                EditorApplication.LockReloadAssemblies();
                var installButton = obj.currentTarget as Button;
                var selection = installButton.userData as PackageGroup;
                var packageName = PackageHelper.GetCleanPackageName(selection.DependencyId.ToLower());
                if (selection.Installed)
                {
                    ScriptingSymbolManager.RemoveScriptingDefine(packageName);
                    deletePackage = CreateInstance<DeletePackage>();
                    deletePackage.directory = selection.InstallDirectory;
                    TryDelete();
                }
                else
                {
                    await selection.Source.InstallPackage(selection, targetVersion);
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }
        }

        private void TryDelete()
        {
            if (deletePackage && deletePackage.TryDelete())
            {
                var deletePackage = this.deletePackage;
                this.deletePackage = null;
                DestroyImmediate(deletePackage);
                AssetDatabase.Refresh();
            }
        }

        void BindVersionButton(PackageGroup selection, Button versionButton)
        {
            versionButton.clickable.clickedWithEventInfo -= PickVersion;
            versionButton.clickable.clickedWithEventInfo += PickVersion;
            versionButton.userData = selection;
            versionButton.text = targetVersion;
            versionButton.SetEnabled(!selection.Installed);
        }

        void PickVersion(EventBase obj)
        {
            var versionButton = obj.currentTarget as Button;
            var selection = versionButton.userData as PackageGroup;
            var menu = new GenericMenu();
            foreach (var version in selection.Versions)
            {
                menu.AddItem(new GUIContent(version.version), version.Equals(targetVersion), SelectVersion, new SelectData(version, versionButton));
            }
            menu.ShowAsContext();
        }

        private class SelectData
        {
            public PackageVersion version;
            public Button versionButton;
            public SelectData(PackageVersion version, Button versionButton)
            {
                this.version = version;
                this.versionButton = versionButton;
            }
        }

        void SelectVersion(object userData)
        {
            var selectData = (SelectData)userData;
            selectData.versionButton.text = targetVersion = selectData.version.version;
        }
        #endregion

    }
}
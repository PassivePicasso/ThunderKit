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
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
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
            UpdatePackageList();
        }

        private void RefreshClicked()
        {
            PackageSource.LoadAllSources();
        }

        private void ConstructPackageSourceList(List<PackageSource> packageSources)
        {
            var packageSourceList = rootVisualElement.Q(name = "tkpm-package-source-list");

            packageSourceList.Clear();

            for (int sourceIndex = 0; sourceIndex < packageSources.Count; sourceIndex++)
            {
                var source = packageSources[sourceIndex];
                var packageSource = GetTemplateInstance("PackageSource");
                var packageList = packageSource.Q<ListView>("tkpm-package-list");
                var foldOut = packageSource.Q<Foldout>();
                var groupName = $"tkpm-package-source-{NormalizeName(source.name)}";
                packageSource.RemoveFromClassList("grow");
                foldOut.value = false;
                foldOut.RegisterCallback<ChangeEvent<bool>>((evt) =>
                {
                    if (evt.newValue)
                        packageSource.AddToClassList("grow");
                    else
                        packageSource.RemoveFromClassList("grow");
                });
                var loadingIndicator = packageSource.Q<LoadingSpinner>(name: "tkpm-package-source-loading-indicator");
                source.OnLoadingStarted += () => loadingIndicator.Start();
                source.OnLoadingStopped += () => loadingIndicator.Stop();

                packageSource.AddToClassList("tkpm-package-source");
                packageSource.name = groupName;
                packageSource.userData = source;

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
                packageList.bindItem = BindPackage;

                packageSourceList.Add(packageSource);
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
                var allTags = pkgs?.SelectMany(pkg => pkg.Tags);
                var distinctTags = allTags?.Distinct();
                var pathedTags = distinctTags?.Select(tag => $"{source.Name}/{tag}");
                var tags = (pathedTags ?? Enumerable.Empty<string>());
                foreach (var tag in tags)
                    if (tagEnabled.ContainsKey(tag)) tagEnabled[tag] = tagEnabled[tag];
                    else
                        tagEnabled[tag] = false;

                headerLabel.text = $"{source.name} ({packageList.itemsSource.Count} packages) ({source.Packages.Count - packageList.itemsSource.Count} hidden)";
            }
        }

        List<PackageGroup> FilterPackages(List<PackageGroup> packages)
        {
            var enabledTags = tagEnabled.Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key.Substring(kvp.Key.LastIndexOf('/') + 1))
                .ToArray();

            packages = packages.Where(pkg => pkg).ToList();

            var hasTags = enabledTags.Any() ? packages.Where(pkg => enabledTags.All(tag => ArrayUtility.Contains(pkg.Tags, tag))) : packages;
            if (!hasTags.Any())
                return hasTags.ToList();

            var hasString = hasTags.Where(pkg => pkg.HasString(SearchString ?? string.Empty));
            if (!hasString.Any())
                return hasString.ToList();

            var filteredByInstall = !InProject ? hasString : hasString.Where(pkg => pkg.Installed);

            return filteredByInstall.ToList();
        }

        void BindPackage(VisualElement packageElement, int packageIndex)
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
                   packageName.text =
                    NicifyPackageName(package.PackageName);

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
            if (selection.Installed)
                targetVersion = PackageHelper.GetPackageManagerManifest(selection.InstallDirectory).version;
            else
                targetVersion = selection["latest"].version;

            ConfigureVersionButton(packageView.Q<Button>("tkpm-package-version-button"), selection);
            ConfigureInstallButton(packageView.Q<Button>("tkpm-package-install-button"), selection);

            RepopulateLabels(packageView.Q("tkpm-package-tags"), selection.Tags, "tag");

            var selectedVersion = selection[targetVersion];
            var pvDependencies = selectedVersion?.dependencies ?? EmptyPackages;
            var dependencyIds = new List<string>();
            foreach (var pvd in pvDependencies.Where(pv => pv != null))
            {
                dependencyIds.Add(pvd.dependencyId);
            }
            var texts = dependencyIds ?? Enumerable.Empty<string>();
            RepopulateLabels(packageView.Q("tkpm-package-dependencies"), texts, "dependency");

            SetLabel(packageView, "tkpm-package-title", NicifyPackageName(selection.PackageName));
            SetLabel(packageView, "tkpm-package-name", selection.DependencyId);
            if (selection.Installed)
                SetLabel(packageView, "tkpm-package-info-version-value", selection.InstalledVersion);
            else
                SetLabel(packageView, "tkpm-package-info-version-value", selection["latest"].version);

            SetLabel(packageView, "tkpm-package-author-value", selection.Author);
            SetLabel(packageView, "tkpm-package-description", selection.Description);
        }

        void SetLabel(VisualElement root, string name, string text)
        {
            var label = root.Q<Label>(name);
            if (label != null) label.text = text;
        }

        void RepopulateLabels(VisualElement container, IEnumerable<string> texts, params string[] classes)
        {
            container.Clear();
            foreach (var text in texts)
            {
                if (string.IsNullOrEmpty(text)) continue;
                var label = new Label(text);
                foreach (var clss in classes)
                    if (!string.IsNullOrEmpty(clss))
                        label.AddToClassList(clss);
                container.Add(label);
            }
        }

        #region Installation
        void ConfigureInstallButton(Button installButton, PackageGroup selection)
        {
            installButton.userData = selection;
            installButton.clickable.clickedWithEventInfo -= InstallVersion;
            installButton.clickable.clickedWithEventInfo += InstallVersion;
            installButton.text = selection.Installed ? "Uninstall" : "Install";
        }

        async void InstallVersion(EventBase obj)
        {
            var installButton = obj.currentTarget as Button;
            var selection = installButton.userData as PackageGroup;
            if (selection.Installed)
            {
                var packageName = selection.PackageManifest.name;
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

        void ConfigureVersionButton(Button versionButton, PackageGroup selection)
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
                menu.AddItem(new GUIContent(version.version), version.Equals(targetVersion), SelectVersion, new SelectData(version.version, versionButton));
            }
            menu.ShowAsContext();
        }

        private class SelectData
        {
            public string version;
            public Button versionButton;
            public SelectData(string version, Button versionButton)
            {
                this.version = version;
                this.versionButton = versionButton;
            }
        }

        void SelectVersion(object userData)
        {
            var selectData = (SelectData)userData;

            selectData.versionButton.text = targetVersion = selectData.version;
        }
        #endregion

    }
}
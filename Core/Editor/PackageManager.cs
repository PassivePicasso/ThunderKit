using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Common;
using ThunderKit.Common.Package;
using ThunderKit.Core.Data;
using ThunderKit.Core.Editor.Actions;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using PackageSource = ThunderKit.Core.Data.PackageSource;

namespace ThunderKit.Core.Editor
{
    public class PackageManager : EditorWindow
    {
        Dictionary<string, VisualTreeAsset> templateCache = new Dictionary<string, VisualTreeAsset>();
        static string[] searchpaths = new string[] { "Assets", "Packages" };
        private static PackageManager wnd;
        private VisualElement root;
        private VisualElement packageView;
        private TextField searchBox;
        private Button searchBoxCancel;
        Dictionary<string, bool> tagEnabled = new Dictionary<string, bool>();
        [SerializeField] public bool InProject;
        [SerializeField] private DeletePackage deletePackage;
        [SerializeField] private string SearchString;
        private Button filtersButton;
        private string targetVersion;


        [MenuItem(Constants.ThunderKitMenuRoot + "Package Manager")]
        public static void ShowExample()
        {
            wnd = GetWindow<PackageManager>();
            wnd.titleContent = new GUIContent("ThunderKit Packages");
        }

        public void OnEnable()
        {
            Construct();
        }

        private void Construct()
        {
            root = this.GetRootVisualContainer();
            root.Clear();

            GetTemplateInstance("PackageManagerData", root);

            packageView = root.Q("tkpm-package-view");
            searchBox = root.Q<TextField>("tkpm-search-textfield");
            searchBoxCancel = root.Q<Button>("tkpm-search-cancelbutton");
            filtersButton = root.Q<Button>("tkpm-filters-selector");

            searchBox.RegisterCallback<ChangeEvent<string>>(OnSearchText);
            searchBox.SetValueWithoutNotify(SearchString);

            filtersButton.clickable.clicked -= FiltersClicked;
            filtersButton.clickable.clicked += FiltersClicked;

            GetTemplateInstance("PackageView", packageView);

            var packageSourceList = root.Q(name = "tkpm-package-source-list");
            var packageSources = AssetDatabase.FindAssets("t:PackageSource", new[] { "Assets", "Packages" })
                                              .Select(AssetDatabase.GUIDToAssetPath)
                                              .Select(AssetDatabase.LoadAssetAtPath<PackageSource>)
                                              .ToArray();

            for (int sourceIndex = 0; sourceIndex < packageSources.Length; sourceIndex++)
            {
                var source = packageSources[sourceIndex];
                var packageSource = GetTemplateInstance("PackageSource");
                var packageList = packageSource.Q<ListView>("tkpm-package-list");
                var groupName = $"tkpm-package-source-{source.Name}";

                packageSource.AddToClassList("tkpm-package-source");
                packageSource.name = groupName;
                packageSource.userData = source;

                packageList.selectionType = SelectionType.Single;
                packageList.onSelectionChanged -= PackageList_onSelectionChanged;
                packageList.onSelectionChanged += PackageList_onSelectionChanged;

                packageList.makeItem = MakePackage;
                VisualElement MakePackage()
                {
                    var packageInstance = GetTemplateInstance("Package");
                    packageInstance.userData = packageList;
                    packageInstance.AddToClassList("tkpm-package-option");
                    return packageInstance;
                }
                packageList.bindItem = BindPackage;

                packageSourceList.Add(packageSource);

                UpdatePackageList();
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
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
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        void UpdatePackageList()
        {
            var packageSources = AssetDatabase.FindAssets("t:PackageSource", new[] { "Assets", "Packages" })
                                              .Select(AssetDatabase.GUIDToAssetPath)
                                              .Select(AssetDatabase.LoadAssetAtPath<PackageSource>)
                                              .ToArray();
            for (int sourceIndex = 0; sourceIndex < packageSources.Length; sourceIndex++)
            {
                var source = packageSources[sourceIndex];
                var packageSource = root.Q($"tkpm-package-source-{source.Name}");
                var headerLabel = packageSource.Q<Label>("tkpm-package-source-label");
                var packageList = packageSource.Q<ListView>("tkpm-package-list");

                packageList.itemsSource = FilterPackages(source.Packages);

                packageList.Refresh();

                if (sourceIndex == 0 && packageList.itemsSource.Count > 0)
                {
                    packageList.selectedIndex = 0;
                    BindPackageView(packageList.selectedItem as PackageGroup);
                }

                var tags = source.Packages.SelectMany(pkg => pkg.Tags).Distinct().Select(tag => $"{source.Name}/{tag}");
                foreach (var tag in tags)
                    if (tagEnabled.ContainsKey(tag)) tagEnabled[tag] = tagEnabled[tag];
                    else
                        tagEnabled[tag] = false;

                headerLabel.text = $"{source.Name} ({packageList.itemsSource.Count} packages) ({source.Packages.Count - packageList.itemsSource.Count} hidden)";

            }
        }

        List<PackageGroup> FilterPackages(List<PackageGroup> packages)
        {
            var enabledTags = tagEnabled.Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key.Substring(kvp.Key.LastIndexOf('/') + 1))
                .ToArray();

            var hasTags = enabledTags.Any() ? packages.Where(pkg => enabledTags.All(tag => pkg.Tags.Contains(tag))) : packages;

            var hasString = hasTags.Where(pkg => pkg.HasString(SearchString ?? string.Empty));

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
        private void PackageList_onSelectionChanged(List<object> obj)
        {
            var selection = obj.OfType<PackageGroup>().First();
            if (selection == null) return;
            BindPackageView(selection);
        }

        private void BindPackageView(PackageGroup selection)
        {
            var title = packageView.Q<Label>("tkpm-package-title");
            var name = packageView.Q<Label>("tkpm-package-name");
            var description = packageView.Q<Label>("tkpm-package-description");
            var versionLabel = packageView.Q<Label>("tkpm-package-info-version-value");
            var author = packageView.Q<Label>("tkpm-package-author-value");
            var versionButton = packageView.Q<Button>("tkpm-package-version-button");
            var installButton = packageView.Q<Button>("tkpm-package-install-button");
            var tags = packageView.Q("tkpm-package-tags");
            var dependencies = packageView.Q("tkpm-package-dependencies");

            if (selection.Installed)
                targetVersion = PackageHelper.GetPackageManagerManifest(selection.PackageDirectory).version;
            else
                targetVersion = selection["latest"].version;

            ConfigureVersionButton(versionButton, selection);
            ConfigureInstallButton(installButton, selection);

            tags.Clear();
            foreach (var tag in selection.Tags)
            {
                var tagLabel = new Label(tag);
                tagLabel.AddToClassList("tag");
                tags.Add(tagLabel);
            }
            dependencies.Clear();
            foreach (var dependency in selection[targetVersion].dependencies)
            {
                var dependencyLabel = new Label(dependency?.dependencyId);
                dependencyLabel.AddToClassList("dependency");
                dependencies.Add(dependencyLabel);
            }

            title.text = NicifyPackageName(selection.PackageName);
            name.text = selection.DependencyId;
            if (selection.Installed)
                versionLabel.text = selection.InstalledVersion;
            else
                versionLabel.text = selection["latest"].version;

            author.text = selection.Author;
            description.text = selection.Description;
        }

        #region Installation
        void ConfigureInstallButton(Button installButton, PackageGroup selection)
        {
            installButton.userData = selection;
            installButton.clickable.clickedWithEventInfo -= InstallVersion;
            installButton.clickable.clickedWithEventInfo += InstallVersion;
            installButton.text = selection.Installed ? "Uninstall" : "Install";
        }

        void InstallVersion(EventBase obj)
        {
            var installButton = obj.currentTarget as Button;
            var selection = installButton.userData as PackageGroup;
            if (selection.Installed)
            {
                deletePackage = CreateInstance<DeletePackage>();
                deletePackage.directory = selection.PackageDirectory;
                TryDelete();
                AssetDatabase.Refresh();
            }
            else
                _ = selection.Source.InstallPackage(selection, targetVersion);
        }
        private void OnInspectorUpdate()
        {
            TryDelete();
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
                menu.AddItem(new GUIContent(version.version), version.Equals(targetVersion), SelectVersion, (version.version, versionButton));
            }
            menu.ShowAsContext();
        }

        void SelectVersion(object userData)
        {
            var (version, versionButton) = ((string, Button))userData;

            versionButton.text = targetVersion = version;
        }
        #endregion

        private static string NicifyPackageName(string name) => ObjectNames.NicifyVariableName(name).Replace("_", " ");

        VisualElement GetTemplateInstance(string template, VisualElement target = null)
        {
            var packageTemplate = LoadTemplate(template);
            var assetPath = AssetDatabase.GetAssetPath(packageTemplate);
            VisualElement instance = target;
            if (instance == null) instance = packageTemplate.CloneTree(null);
            else
                packageTemplate.CloneTree(instance, null);

            instance.AddToClassList("grow");
            AddSheet(instance, assetPath);
            if (EditorGUIUtility.isProSkin)
                AddSheet(instance, assetPath, "_Dark");
            else
                AddSheet(instance, assetPath, "_Light");

            return instance;
        }

        void AddSheet(VisualElement element, string assetPath, string modifier = "")
        {
            string sheetPath = assetPath.Replace(".uxml", $"{modifier}.uss");
            if (File.Exists(sheetPath)) element.AddStyleSheetPath(sheetPath);
        }

        private VisualTreeAsset LoadTemplate(string name)
        {
            if (!templateCache.ContainsKey(name))
            {
                var searchResults = AssetDatabase.FindAssets(name, searchpaths);
                var assetPaths = searchResults.Select(AssetDatabase.GUIDToAssetPath);
                var templatePath = assetPaths.FirstOrDefault(path => path.Contains("Templates/") || path.Contains("Templates\\"));
                templateCache[name] = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(templatePath);
            }
            return templateCache[name];
        }
    }
}
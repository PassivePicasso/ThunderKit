using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Common;
using ThunderKit.Common.Package;
using ThunderKit.Core.Editor;
using ThunderKit.PackageManager.Engine;
using ThunderKit.PackageManager.Model;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using PackageSource = ThunderKit.PackageManager.Model.PackageSource;

namespace ThunderKit.PackageManager.Editor
{
    public class ThunderKitPackageManager : EditorWindow
    {
        Dictionary<string, VisualTreeAsset> templateCache = new Dictionary<string, VisualTreeAsset>();
        static string[] searchpaths = new string[] { "Assets", "Packages" };
        private static ThunderKitPackageManager wnd;
        private static List<PackageSource> packageSources;
        private VisualElement root;
        private VisualElement packageView;
        private TextField searchBox;
        private Button searchBoxCancel;

        [SerializeField] public bool InProject;
        [SerializeField] private DeletePackage deletePackage;
        [SerializeField] private string SearchString;
        private Button filtersButton;

        public static void RegisterPackageSource(PackageSource source)
        {
            if (packageSources == null)
                packageSources = new List<PackageSource>();

            packageSources.Add(source);
        }

        [MenuItem(Constants.ThunderKitMenuRoot + "Package Manager")]
        public static void ShowExample()
        {
            wnd = GetWindow<ThunderKitPackageManager>();
            wnd.titleContent = new GUIContent("ThunderKit Packages");
        }

        public void OnEnable()
        {
            Construct();
        }

        private void OnInspectorUpdate()
        {
            TryDelete();
        }

        private void TryDelete()
        {
            if (deletePackage)
            {
                if (deletePackage.TryDelete())
                {
                    DestroyImmediate(deletePackage);
                    deletePackage = null;
                    AssetDatabase.Refresh();
                }
            }
        }

        public static PackageManagerData GetOrCreateSettings()
        {
            string assetPath = $"{Constants.ThunderKitSettingsRoot}{typeof(PackageManagerData).Name}.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            return ScriptableHelper.EnsureAsset<PackageManagerData>(assetPath, settings => { });
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

            for (int sourceIndex = 0; sourceIndex < packageSources.Count; sourceIndex++)
            {
                var source = packageSources[sourceIndex];
                var sourceList = GetPackageSourceList(source);
                var packageSource = GetTemplateInstance("PackageSource");
                var packageList = packageSource.Q<ListView>("tkpm-package-list");
                var groupName = $"tkpm-package-source-{sourceList.SourceName}";

                packageSource.AddToClassList("tkpm-package-source");
                packageSource.name = groupName;
                packageSource.userData = sourceList;

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
        }

        private void FiltersClicked()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(nameof(InProject))), InProject, () =>
            {
                InProject = !InProject;
                UpdatePackageList();
            });
            menu.ShowAsContext();
        }

        private void UpdatePackageSource(PackageSourceList sourceList, PackageSource source)
        {
            if (sourceList.packages == null || !sourceList.packages.Any() || (DateTime.Now - sourceList.lastUpdateTime) > TimeSpan.FromSeconds(300))
            {
                var packages = source.GetPackages(string.Empty);
                if (packages != null && packages.Any())
                {
                    sourceList.packages = packages.ToList();
                    sourceList.lastUpdateTime = DateTime.Now;
                    EditorUtility.SetDirty(sourceList);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private void OnSearchText(ChangeEvent<string> evt)
        {
            SearchString = evt.newValue;
            UpdatePackageList();
        }

        void UpdatePackageList()
        {
            for (int sourceIndex = 0; sourceIndex < packageSources.Count; sourceIndex++)
            {
                var source = packageSources[sourceIndex];
                var sourceList = GetPackageSourceList(source);

                UpdatePackageSource(sourceList, source);

                var packageSource = root.Q($"tkpm-package-source-{source.GetName()}");
                var headerLabel = packageSource.Q<Label>("tkpm-package-source-label");
                var packageList = packageSource.Q<ListView>("tkpm-package-list");

                packageList.itemsSource = FilterPackages(sourceList.packages);

                if (packageList.selectedIndex < 0 && sourceIndex == 0 && packageList.itemsSource.Count > 0)
                    packageList.selectedIndex = 0;

                packageList.Refresh();
                
                headerLabel.text = $"{sourceList.SourceName} ({packageList.itemsSource.Count} packages) ({sourceList.packages.Count - packageList.itemsSource.Count} hidden)";

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        List<PackageGroup> FilterPackages(List<PackageGroup> packages) => packages
                    .Where(pkg => pkg.HasString(SearchString ?? string.Empty))
                    .Where(pkg => (!InProject || PackageInstalled(pkg, pkg.version)))
                    .ToList();

        void BindPackage(VisualElement packageElement, int packageIndex)
        {
            var sourceList = packageElement.userData as ListView;
            var package = sourceList.itemsSource[packageIndex] as PackageGroup;
            packageElement.name = $"tkpm-package-{package.name}-{package.version}";

            var packageInstalled = packageElement.Q<Image>("tkpm-package-installed");

            if (PackageInstalled(package, package.version)) packageInstalled.AddToClassList("installed");
            else
                packageInstalled.RemoveFromClassList("installed");

            var packageName = packageElement.Q<Label>("tkpm-package-name");
            if (packageName != null)
                packageName.tooltip =
                   packageName.text =
                    NicifyPackageName(package.name);

            var packageVersion = packageElement.Q<Label>("tkpm-package-version");
            if (packageVersion != null) packageVersion.text = package.version;
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

            ConfigureVersionButton(versionButton, selection);
            ConfigureInstallButton(installButton, versionButton, selection);

            title.text = NicifyPackageName(selection.name);
            name.text = selection.dependencyId;
            versionLabel.text = selection.version;
            author.text = selection.author;
            description.text = selection.description;
        }

        void ConfigureInstallButton(Button installButton, Button versionButton, PackageGroup selection)
        {
            versionButton.userData = selection;
            installButton.userData = versionButton;
            installButton.clickable.clickedWithEventInfo -= InstallVersion;
            installButton.clickable.clickedWithEventInfo += InstallVersion;
            installButton.text = PackageInstalled(selection, selection.version) ? "Uninstall" : "Install";
        }

        void InstallVersion(EventBase obj)
        {
            var installButton = obj.currentTarget as Button;
            var versionButton = installButton.userData as Button;
            var selection = versionButton.userData as PackageGroup;
            var packageVersion = selection.versions.First(pv => pv.version.Equals(versionButton.text));

            var packageDirectory = Path.Combine("Packages", selection.name);
            if (PackageInstalled(selection, selection.version))
            {
                deletePackage = CreateInstance<DeletePackage>();
                deletePackage.directory = packageDirectory;
                TryDelete();
                AssetDatabase.Refresh();
            }
            else
                selection.Source.InstallPackage(selection, versionButton.text, packageDirectory);
        }

        void ConfigureVersionButton(Button versionButton, PackageGroup selection)
        {
            versionButton.clickable.clickedWithEventInfo -= PickVersion;
            versionButton.clickable.clickedWithEventInfo += PickVersion;
            versionButton.userData = selection;
            var directory = PackageDirectory(selection);
            versionButton.SetEnabled(!PackageInstalled(selection, selection.version));

            if (PackageInstalled(selection, selection.version))
                versionButton.text = PackageHelper.GetPackageManagerManifest(directory).version;
            else
                versionButton.text = selection.version;
        }

        void PickVersion(EventBase obj)
        {
            var versionButton = obj.currentTarget as Button;
            var selection = versionButton.userData as PackageGroup;
            var menu = new GenericMenu();
            foreach (var packageVersion in selection.versions)
            {
                void SelectVersion()
                {
                    versionButton.text = packageVersion.version;
                    BindPackageView(selection);
                }

                menu.AddItem(new GUIContent(packageVersion.version), packageVersion.version.Equals(versionButton.text), SelectVersion);
            }
            menu.ShowAsContext();
        }

        PackageSourceList GetPackageSourceList(PackageSource source) => ScriptableHelper.EnsureAsset<PackageSourceList>(
                                    $"{Constants.ThunderKitSettingsRoot}{source.GetName()}_SourceSettings.asset",
                                    psl => psl.SourceName = source.GetName());

        private static string PackageDirectory(PackageGroup package)
        {
            return Directory.EnumerateDirectories("Packages", package.name, SearchOption.TopDirectoryOnly).FirstOrDefault();
        }

        private static bool PackageInstalled(PackageGroup package, string version)
        {
            string directory = PackageDirectory(package);
            if (string.IsNullOrEmpty(directory)) return false;

            var pmm = PackageHelper.GetPackageManagerManifest(directory);
            var packageVersion = package[version];

            return pmm.name.Equals(packageVersion.dependencyId, System.StringComparison.OrdinalIgnoreCase);
        }

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
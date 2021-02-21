using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Common;
using ThunderKit.Core.Editor;
using ThunderKit.PackageManager.Model;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using VisualTemplates;

namespace ThunderKit.PackageManager.Editor
{
    public class ThunderKitPackageManager : EditorWindow
    {
        static string[] searchpaths = new string[] { "Assets", "Packages" };
        private static ThunderKitPackageManager wnd;
        private static List<PackageSource> packageSources;
        private VisualElement root;
        private ContentPresenter presenter;
        private string SearchString, lastSearch;
        public bool FilterInstalled;

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
            if (root == null) Construct();
        }

        public static PackageManagerData GetOrCreateSettings()
        {
            string assetPath = $"{Constants.ThunderKitSettingsRoot}{typeof(PackageManagerData).Name}.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            return ScriptableHelper.EnsureAsset<PackageManagerData>(assetPath, settings => { });
        }

        private void Construct(bool select = true)
        {
            ContentPresenter.DefaultLoadAsset = LoadTemplate;
            root = this.GetRootVisualContainer();
            presenter = new ContentPresenter { name = "package-manager-presenter" };
            presenter.RegisterCallback<AttachToPanelEvent>(OnRootPresenterAttach);
            root.Add(presenter);
            root.Bind(new SerializedObject(GetOrCreateSettings()));

            packageView = root.Q<ContentPresenter>("tkpm-package-view");
            searchBox = root.Q<TextField>("tkpm-search-textfield");
            searchBoxCancel = root.Q<Button>("tkpm-search-cancelbutton");
            searchBox.RegisterCallback<ChangeEvent<string>>(OnSearchText);
        }

        private void OnSearchText(ChangeEvent<string> evt)
        {
            SearchString = evt.newValue;
            foreach (var source in packageSources)
            {
                var sourceList = ScriptableHelper.EnsureAsset<PackageSourceList>(
                                    $"{Constants.ThunderKitSettingsRoot}{source.GetName()}_SourceSettings.asset",
                                    psl => psl.SourceName = source.GetName());

                var packages = source.GetPackages(SearchString ?? string.Empty);
                if (FilterInstalled)
                    packages = packages.Where(PackageInstalled);

                sourceList.packages = packages.ToList();
                var sourcePresenter = presenter.Q<ContentPresenter>($"tkpm-package-source-{source.GetName()}");
                var headerLabel = sourcePresenter.Q<Label>("tkpm-package-source-label");
                headerLabel.text = $"{sourceList.SourceName} ({sourceList.packages.Count} packages)";
                var packageList = sourcePresenter.Q<ListView>("tkpm-package-list");
                packageList.itemsSource = sourceList.packages;
            }
        }

        private void OnRootPresenterAttach(AttachToPanelEvent evt)
        {
            presenter.UnregisterCallback<AttachToPanelEvent>(OnRootPresenterAttach);

            var packageSourceList = presenter.Q(name = "tkpm-package-source-list");

            foreach (var source in packageSources)
            {
                var sourcePresenter = new ContentPresenter { name = $"tkpm-{source.GetName()}-presenter", userData = this, Template = "PackageSource" };
                packageSourceList.Add(sourcePresenter);

                sourcePresenter.AddToClassList("tkpm-package-source");

                var sourceList = ScriptableHelper.EnsureAsset<PackageSourceList>(
                                    $"{Constants.ThunderKitSettingsRoot}{source.GetName()}_SourceSettings.asset",
                                    psl => psl.SourceName = source.GetName());

                var packages = source.GetPackages(SearchString ?? string.Empty);
                if (FilterInstalled)
                    packages = packages.Where(PackageInstalled);
                sourceList.packages = packages.ToList();

                var groupName = $"tkpm-package-source-{sourceList.SourceName}";
                sourcePresenter.name = groupName;
                sourcePresenter.userData = sourceList;
                sourcePresenter.Bind(new SerializedObject(sourceList));

                var headerLabel = sourcePresenter.Q<Label>("tkpm-package-source-label");
                headerLabel.text = $"{sourceList.SourceName} ({sourceList.packages.Count} packages)";

                var packageList = sourcePresenter.Q<ListView>("tkpm-package-list");
                if (packageList == null) return;
                packageList.selectionType = SelectionType.Single;
                packageList.RegisterCallback<AttachToPanelEvent>(OnPackageListAttached);
                packageList.onSelectionChanged -= PackageList_onSelectionChanged;
                packageList.onSelectionChanged += PackageList_onSelectionChanged;
                //packageList.StretchToParentWidth();
                void OnPackageListAttached(AttachToPanelEvent et)
                {
                    packageList.makeItem = MakePackage;
                    VisualElement MakePackage()
                    {
                        var packageTemplate = LoadTemplate("Package");
                        var packageInstance = packageTemplate.CloneTree(null);
                        packageInstance.userData = sourceList;
                        packageInstance.AddToClassList("tkpm-package-option");
                        packageInstance.AddStyleSheetPath(AssetDatabase.GetAssetPath(packageTemplate).Replace(".uxml", ".uss"));
                        packageInstance.AddStyleSheetPath(AssetDatabase.GetAssetPath(packageTemplate).Replace(".uxml", "_Light.uss"));
                        packageInstance.AddStyleSheetPath(AssetDatabase.GetAssetPath(packageTemplate).Replace(".uxml", "_Dark.uss"));
                        return packageInstance;
                    }
                    packageList.bindItem = BindPackage;
                    packageList.itemsSource = sourceList.packages;
                }
            }
        }

        void BindPackage(VisualElement packageElement, int packageIndex)
        {
            var sourceList = packageElement.userData as PackageSourceList;
            var package = sourceList.packages[packageIndex];
            packageElement.name = $"tkpm-package-{package.name}-{package.version}";
            //packageElement.Bind(new SerializedObject(sourceList));
            var packageInstalled = packageElement.Q<Image>("tkpm-package-installed");

            if (PackageInstalled(package)) packageInstalled.AddToClassList("installed");
            else
                packageInstalled.RemoveFromClassList("installed");

            //if (PackageCanUpdate(package)) packageInstalled.AddToClassList("update");
            //else
            //    packageInstalled.RemoveFromClassList("update");

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
            var title = packageView.Q<Label>("tkpm-package-title");
            var name = packageView.Q<Label>("tkpm-package-name");
            var description = packageView.Q<Label>("tkpm-package-description");
            var versionLabel = packageView.Q<Label>("tkpm-package-info-version-value");
            var author = packageView.Q<Label>("tkpm-package-author-value");
            var versionButton = packageView.Q<Button>("tkpm-package-version-button");

            ConfigureVersionButton(versionButton, selection);
            ConfigureInstallButton(packageView.Q<Button>("tkpm-package-version-button"), versionButton, selection);

            title.text = NicifyPackageName(selection.name);
            name.text = selection.dependencyId;
            versionLabel.text = selection.version;
            author.text = selection.author;
            description.text = selection.description;
        }

        void ConfigureInstallButton(Button installButton, Button versionButton, PackageGroup selection)
        {
            installButton.clickable.clicked -= InstallVersion;
            installButton.clickable.clicked += InstallVersion;
            void InstallVersion() => selection.Source.InstallPackage(selection, versionButton.text, Path.Combine("Temp", "ThunderKit", "PackageStaging"));
        }

        void ConfigureVersionButton(Button versionButton, PackageGroup selection)
        {
            versionButton.clickable.clicked -= PickVersion;
            versionButton.clickable.clicked += PickVersion;
            void PickVersion()
            {
                var menu = new GenericMenu();
                foreach (var packageVersion in selection.versions)
                {
                    void SelectVersion()
                    {
                        versionButton.text = packageVersion.version;
                    }
                    menu.AddItem(new GUIContent(packageVersion.version), packageVersion.version.Equals(versionButton.text), SelectVersion);
                }
                menu.ShowAsContext();
            }
            versionButton.text = selection.version;
        }

        private static bool PackageInstalled(PackageGroup package) => Directory.EnumerateDirectories("Packages", $"{package.dependencyId}*", SearchOption.TopDirectoryOnly).Any();
        //private static bool PackageCanUpdate(PackageGroup package) => !PackageInstalled(package) && Directory.Exists(Path.Combine("Packages", package.name));

        private static string NicifyPackageName(string name) => ObjectNames.NicifyVariableName(name).Replace("_", " ");


        Dictionary<string, VisualTreeAsset> templateCache = new Dictionary<string, VisualTreeAsset>();
        private ContentPresenter packageView;
        private TextField searchBox;
        private Button searchBoxCancel;

        private VisualTreeAsset LoadTemplate(string name)
        {
            if (!templateCache.ContainsKey(name))
            {
                templateCache[name] = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase
                   .FindAssets(name, searchpaths)
                   .Select(AssetDatabase.GUIDToAssetPath)
                   .FirstOrDefault(path => path.Contains("Templates/") || path.Contains("Templates\\")));
            }
            return templateCache[name];
        }
    }
}
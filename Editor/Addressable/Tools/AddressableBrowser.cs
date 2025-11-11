#if TK_ADDRESSABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ThunderKit.Common;
using ThunderKit.Core.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine.Rendering;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif
using Object = UnityEngine.Object;

namespace ThunderKit.Addressable.Tools
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;
    [Flags]
    public enum BrowserOptions
    {
        None = 0x0,
        ShowType = 0x1,
        ShowProvider = 0x2,
        IgnoreCase = 0x4,
        UseRegex = 0x8
    }

    public class AddressableBrowser : TemplatedWindow
    {
        [MenuItem(Constants.ThunderKitMenuRoot + "Addressable Browser")]
        public static void ShowAddressableBrowser() => GetWindow<AddressableBrowser>();

        public override string Title { get => ObjectNames.NicifyVariableName(GetType().Name); }

        private const string CopyButton = "addressable-copy-button";
        private const string NameLabel = "addressable-label";
        private const string TypeLabel = "addressable-type-label";
        private const string ProviderLabel = "addressable-provider-label";
        private const string PreviewIcon = "addressable-icon";
        private const string AddressableAssetName = "addressable-asset";
        private const string ButtonPanel = "addressable-button-panel";
        private const string LoadSceneButton = "addressable-load-scene-button";
        private const string InspectButton = "addressable-inspect-button";
        private const string AddressableLabels = "addressable-labels";

        Dictionary<string, List<IResourceLocation>> DirectoryContents;

        private ListView directory;
        private ListView directoryContent;
        private TextField searchBox;
        private EnumFlagsField displayOptionsField;
        private Button helpButton;
        public string searchInput;
        public BrowserOptions browserOptions = BrowserOptions.ShowType | BrowserOptions.ShowProvider | BrowserOptions.IgnoreCase;

        private object[] loadParams = new object[1];
        private Type[] loadParamTypes = new Type[] { typeof(IResourceLocation) };
        private Regex regex;
        private string typeFilter;
        private List<IResourceLocation> LoadIndex()
        {
            var set = new HashSet<IResourceLocation>();
            var resourceLocations = Addressables.ResourceLocators;
            var locatorIds = resourceLocations.Select(loc => loc.LocatorId).Aggregate((a, b) => $"{a}\n{b}");
            Debug.Log($"Loaded Locators\n{locatorIds}");
            if (!resourceLocations.Any()) return set.ToList();

            foreach (var locator in resourceLocations)
            {
                Debug.Log($"Processing Locator\n{locator.LocatorId}");
                foreach (var location in locator.AllLocations)
                    set.Add(location);
            }
            
            return set.Where(val => val.ResourceType != typeof(IAssetBundleResource)).ToList();
        }

        public void OnDisable() => AddressableGraphicsSettings.AddressablesInitialized -= InitializeBrowser;

        public override void OnEnable()
        {
            AddressableGraphicsSettings.AddressablesInitialized -= InitializeBrowser;
            AddressableGraphicsSettings.AddressablesInitialized += InitializeBrowser;

            InitializeBrowser();
        }


        private void InitializeBrowser(object sender = null, EventArgs e = null)
        {
            base.OnEnable();

            searchBox = rootVisualElement.Q<TextField>("search-input");
            displayOptionsField = rootVisualElement.Q<EnumFlagsField>("display-options");
            directory = rootVisualElement.Q<ListView>("directory");
            directoryContent = rootVisualElement.Q<ListView>("directory-content");
            helpButton = rootVisualElement.Q<Button>("help-button");
            helpButton.clickable = new Clickable(() =>
            {
                Documentation.ShowThunderKitDocumentation("Packages/com.passivepicasso.thunderkit/Documentation/ThunderKitDocumentation/Manual/AddressableBrowser.md");
            });

            directory.itemsSource = new ArrayList();
            directoryContent.itemsSource = new ArrayList();
            directory.makeItem = DirectoryLabel;
            directoryContent.makeItem = LoadAssetTemplate;

            directory.bindItem = (element, i) =>
            {
                Label label = element.Q<Label>(NameLabel);
                var location = (IResourceLocation)directory.itemsSource[i];
                label.text = GroupLocation(location);
            };
            directoryContent.bindItem = BindAsset;

            List<IResourceLocation> resourceLocations = LoadIndex();

            DirectoryContents = resourceLocations.GroupBy(GroupLocation).ToDictionary(g => g.Key, g => g.Distinct().ToList());
            DirectoryContents["Scenes"] = resourceLocations.Where(location => location.ResourceType == typeof(SceneInstance)).ToList();

#if UNITY_2020_1_OR_NEWER
            directory.onSelectionChange += Directory_onSelectionChanged;
            directoryContent.onSelectionChange += DirectoryContent_onSelectionChanged;
#else
            directory.onSelectionChanged += Directory_onSelectionChanged;
            directoryContent.onSelectionChanged += DirectoryContent_onSelectionChanged;
#endif

#if UNITY_2019_1_OR_NEWER
            searchBox.RegisterValueChangedCallback(OnSearchChanged);
            displayOptionsField.RegisterValueChangedCallback(OnOptionsChanged);
#else
            searchBox.OnValueChanged(OnSearchChanged);
            regexOptionsField.OnValueChanged(OnRegexOptionsChanged);
#endif
            RefreshSearch();
        }

        private void OnOptionsChanged(ChangeEvent<System.Enum> evt) => RefreshSearch();
        private void OnSearchChanged(ChangeEvent<string> evt) => RefreshSearch();
        private void RefreshSearch()
        {
            bool noFilter = string.IsNullOrEmpty(searchInput);
            var searchValue = searchInput ?? string.Empty;
            var typeIndex = searchValue.IndexOf("t:");
            if (typeIndex > -1 && (typeIndex - 1 == -1 || searchValue[typeIndex - 1] == ' '))
            {
                typeFilter = searchValue.Substring(typeIndex);
                int spaceIndex = typeFilter.IndexOf(" ");
                if (spaceIndex == -1)
                {
                    searchValue = searchValue.Replace(typeFilter, string.Empty);
                    if (searchValue.EndsWith(" "))
                        searchValue = searchValue.Substring(0, searchValue.Length - 1);
                    typeFilter = typeFilter.Substring(2);
                }
                else
                {
                    searchValue = searchValue.Replace(typeFilter.Substring(0, spaceIndex + 1), string.Empty);
                    typeFilter = typeFilter.Substring(2, spaceIndex - 2);
                }
            }
            else typeFilter = string.Empty;

            if (!browserOptions.HasFlag(BrowserOptions.UseRegex)) searchValue = Regex.Escape(searchValue);

            if (string.IsNullOrEmpty(searchValue))
                regex = null;
            else
            {
                var regexOptions = browserOptions.HasFlag(BrowserOptions.IgnoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None;
                regex = new Regex(searchValue, regexOptions);
            }

            EditorApplication.update += Refresh;
        }
        private void Refresh()
        {
            EditorApplication.update -= Refresh;
            directory.itemsSource.Clear();
            foreach (var kvp in DirectoryContents)
                Filter(directory.itemsSource, kvp.Value, true);

            ((ArrayList)directory.itemsSource).Sort(comparer);

            directory.Rebuild();
            directoryContent.Rebuild();
        }
        ResoourceNameSorter comparer = new ResoourceNameSorter();
        private class ResoourceNameSorter : IComparer
        {
            public int Compare(object x, object y)
            {
                if (x == null || y == null) return 0;
                if (x is IResourceLocation xirl && y is IResourceLocation yirl)
                    return string.Compare(xirl.PrimaryKey, yirl.PrimaryKey);
                return 0;
            }
        }
        private void Directory_onSelectionChanged(IEnumerable<object> obj)
        {
            var selected = GroupLocation(obj.OfType<IResourceLocation>().First());
            var locations = DirectoryContents[selected];

            directoryContent.itemsSource.Clear();
            Filter(directoryContent.itemsSource, locations, false);
            directoryContent.Rebuild();
        }
        private string GroupLocation(IResourceLocation g)
        {
            if (g.ResourceType == typeof(SceneInstance))
            {
                return "Scenes";
            }
            else
            {
                var lastIndexForwardslash = g.PrimaryKey.LastIndexOf("/");
                var lastIndexBackslash = g.PrimaryKey.LastIndexOf("\\");
                if (lastIndexForwardslash > -1 || lastIndexBackslash > -1)
                {
                    if (lastIndexBackslash > lastIndexForwardslash)
                        return g.PrimaryKey.Substring(0, lastIndexBackslash);
                    else
                        return g.PrimaryKey.Substring(0, lastIndexForwardslash);
                }
                else
                    return "Assorted";
            }
        }
        private void Filter(IList list, List<IResourceLocation> values, bool earlyReturn)
        {
            foreach (var location in values)
            {
                if (!typeof(Object).IsAssignableFrom(location.ResourceType) &&
                    !typeof(SceneInstance).IsAssignableFrom(location.ResourceType)) continue;
                if (regex != null && !regex.IsMatch(location.PrimaryKey)) continue;
                if (!string.IsNullOrEmpty(typeFilter) && !location.ResourceType.FullName.Contains(typeFilter)) continue;
                list.Add(location);
                if (earlyReturn)
                    break;
            }
        }
        private void BindAsset(VisualElement element, int i)
        {
            var location = (IResourceLocation)directoryContent.itemsSource[i];

            var icon = element.Q<AddressablePreviewImage>(PreviewIcon);

            var address = location.PrimaryKey;
            var isAsset = IsLoadableAsset(location);

            var nameLabel = element.Q<TextField>(NameLabel);
            nameLabel.SetValueWithoutNotify(address);
            if (browserOptions.HasFlag(BrowserOptions.ShowType))
            {
                var typeLabel = element.Q<TextField>(TypeLabel);
                typeLabel.SetValueWithoutNotify(location.ResourceType.FullName);
            }
            if (browserOptions.HasFlag(BrowserOptions.ShowProvider))
            {
                var providerLabel = element.Q<TextField>(ProviderLabel);
                providerLabel.SetValueWithoutNotify(location.ProviderId);
                switch (location.ProviderId)
                {
                    case "UnityEngine.ResourceManagement.ResourceProviders.LegacyResourcesProvider":
                        providerLabel.tooltip = "Cannot load assets provided by LegacyResourcesProvider";
                        break;
                        //        case "UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider":
                        //            providerLabel.tooltip = location.Dependencies
                        //                .Select(dep => dep.PrimaryKey)
                        //                .Aggregate("Dependencies:",
                        //                 (a, b) => $"{a}\r\n{b}");
                        //            break;
                }
            }

            var loadSceneBtn = element.Q<Button>(LoadSceneButton);
            var inspectBtn = element.Q<Button>(InspectButton);
            inspectBtn.style.display = DisplayStyle.None;
            loadSceneBtn.style.display = DisplayStyle.None;
            if (typeof(GameObject).IsAssignableFrom(location.ResourceType))
            {
                inspectBtn.style.display = DisplayStyle.Flex;
                inspectBtn.clickable = new Clickable(() =>
                {
                    var handle = Addressables.LoadAssetAsync<GameObject>(location);

                    EditorApplication.CallbackFunction updateSceneView = null;
                    updateSceneView = new EditorApplication.CallbackFunction(UpdateSceneView);
                    EditorApplication.update += updateSceneView;
                    void UpdateSceneView()
                    {
                        if (handle.IsDone)
                        {
                            EditorApplication.update -= updateSceneView;
                            AddressablePreviewStage.Show(handle.Result);
                            Addressables.Release(handle);
                        }
                        if (SceneView.lastActiveSceneView)
                        {
                            SceneView.lastActiveSceneView.sceneLighting = false;
                            SceneView.lastActiveSceneView.Repaint();
                        }
                    }
                });
            }
            else if (location.ResourceType == typeof(SceneInstance))
            {
                loadSceneBtn.style.display = DisplayStyle.Flex;
                if (EditorApplication.isPlaying)
                {
                    loadSceneBtn.clickable = new Clickable(() =>
                    {
                        var currentAddress = directoryContent.itemsSource[i] as IResourceLocation;
                        var key = currentAddress.ToString();
                        Addressables.LoadSceneAsync(currentAddress);
                    });
                }
                else
                {
                    loadSceneBtn.clickable = null;
                }
            }

            _ = icon.Render(location, address, isAsset);

        }

        private bool IsLoadableAsset(IResourceLocation location) =>
               location.ResourceType != typeof(SceneInstance)
            && location.ResourceType != typeof(IAssetBundleResource)
            && location.ProviderId != "UnityEngine.ResourceManagement.ResourceProviders.LegacyResourcesProvider"
            && typeof(Object).IsAssignableFrom(location.ResourceType);
        private MethodInfo GetLoadMethod(IResourceLocation location)
        {
            var addressablesType = typeof(Addressables);
            var loadAsyncMethodTemplate = addressablesType.GetMethod(nameof(Addressables.LoadAssetAsync), loadParamTypes);
            var loadAssetAsync = loadAsyncMethodTemplate.MakeGenericMethod(location.ResourceType);
            return loadAssetAsync;
        }
        private void DirectoryContent_onSelectionChanged(IEnumerable<object> obj)
        {
            try
            {
                var location = obj.OfType<IResourceLocation>().First();
                var isLoadable = IsLoadableAsset(location);
                if (!isLoadable)
                    return;

                var loadAssetAsync = GetLoadMethod(location);

                loadParams[0] = location;
                var firstOp = loadAssetAsync.Invoke(null, loadParams);

                var asyncOpType = firstOp.GetType();
                var asyncOpWaitForCompletionMethod = asyncOpType.GetMethod("WaitForCompletion");

                var firstObj = asyncOpWaitForCompletionMethod.Invoke(firstOp, null);
                if (firstObj is Object uniObj)
                {
                    uniObj.hideFlags |= HideFlags.NotEditable;
                    Selection.activeObject = uniObj;
                }
                else
                    Debug.LogWarning("Loaded object is not a UnityEngine.Object");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        private VisualElement DirectoryLabel()
        {
            var label = new Label { name = NameLabel };
            label.AddToClassList("addresable-label");
            return label;
        }
        private VisualElement LoadAssetTemplate()
        {
            var element = GetTemplateInstance("AddressableAsset");
            var labelContainer = element.Q(AddressableLabels);

            if (browserOptions.HasFlag(BrowserOptions.ShowType)) /**/labelContainer.Q(TypeLabel/**/).style.display = DisplayStyle.Flex;
            if (browserOptions.HasFlag(BrowserOptions.ShowProvider)) labelContainer.Q(ProviderLabel).style.display = DisplayStyle.Flex;

            return element;
        }
    }
}
#endif
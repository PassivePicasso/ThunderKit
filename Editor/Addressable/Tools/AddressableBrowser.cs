#if TK_ADDRESSABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderKit.Common;
using ThunderKit.Core.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceLocations;
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
        private const string Library = "Library";
        private const string SimplyAddress = "SimplyAddress";
        private const string Previews = "Previews";

        private static string PreviewRoot => Path.Combine(Library, SimplyAddress, Previews);

        public static readonly Dictionary<string, Texture2D> PreviewCache = new Dictionary<string, Texture2D>();

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
        private const string AddressableLabels = "addressable-labels";

        Dictionary<string, List<IResourceLocation>> DirectoryContents;

        private ListView directory;
        private ListView directoryContent;
        private Texture sceneIcon;
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
            if (!Addressables.ResourceLocators.Any()) return set.ToList();

            foreach (var locator in Addressables.ResourceLocators)
            {
                switch (locator)
                {
                    case ResourceLocationMap rlm:
                        foreach (var location in rlm.Locations.SelectMany(loc => loc.Value).Where(val => val.ResourceType != typeof(IAssetBundleResource)))
                            set.Add(location);
                        break;
                }
            }
            return set.ToList();
        }
        public void OnDisable()
        {
            AddressableGraphicsSettings.AddressablesInitialized -= InitializeBrowser;
        }
        public override void OnEnable()
        {
            AddressableGraphicsSettings.AddressablesInitialized -= InitializeBrowser;
            AddressableGraphicsSettings.AddressablesInitialized += InitializeBrowser;
            InitializeBrowser();
        }


        private void InitializeBrowser(object sender = null, EventArgs e = null)
        {
            base.OnEnable();
            sceneIcon = EditorGUIUtility.IconContent("d_UnityLogo").image;

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
            directoryContent.makeItem = AssetLabel;

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

            directory.onSelectionChanged += Directory_onSelectionChanged;
            directoryContent.onSelectionChanged += DirectoryContent_onSelectionChanged;

#if UNITY_2019_1_OR_NEWER
            searchBox.RegisterValueChangedCallback(OnSearchChanged);
            displayOptionsField.RegisterValueChangedCallback(OnOptionsChanged);
#else
            searchBox.OnValueChanged(OnSearchChanged);
            regexOptionsField.OnValueChanged(OnRegexOptionsChanged);
#endif
            RefreshSearch();
        }

        private void OnUseRegexChanged(ChangeEvent<bool> evt) => RefreshSearch();
        private void OnOptionsChanged(ChangeEvent<System.Enum> evt) => RefreshSearch();
        private void OnSearchChanged(ChangeEvent<string> evt) => RefreshSearch();
        private void MakeReadonlyTextField(VisualElement labelContainer, string name)
        {
            var field = new TextField { name = name, isReadOnly = true };
            field.AddToClassList("addressable-label");
            labelContainer.Add(field);
        }
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

            directory.Refresh();
            directoryContent.Refresh();
        }
        private void Directory_onSelectionChanged(List<object> obj)
        {
            var selected = GroupLocation(obj.OfType<IResourceLocation>().First());
            var locations = DirectoryContents[selected];

            directoryContent.itemsSource.Clear();
            Filter(directoryContent.itemsSource, locations, false);
            directoryContent.Refresh();
        }
        private string GroupLocation(IResourceLocation g)
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
        private void Filter(IList list, List<IResourceLocation> values, bool earlyReturn)
        {
            foreach (var location in values)
            {
                if (regex != null && !regex.IsMatch(location.PrimaryKey)) continue;
                if (!string.IsNullOrEmpty(typeFilter) && !location.ResourceType.FullName.Contains(typeFilter)) continue;
                list.Add(location);
                if (earlyReturn)
                    break;
            }
        }
        private async void BindAsset(VisualElement element, int i)
        {
            var location = (IResourceLocation)directoryContent.itemsSource[i];

            var icon = element.Q<Image>(PreviewIcon);
            var loadSceneBtn = element.Q<Button>(LoadSceneButton);

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

            icon.image = null;
            loadSceneBtn.style.display = DisplayStyle.None;
            if (isAsset)
            {
                var texture = await RenderIcon(address);
                if (texture)
                    icon.image = texture;
            }
            else if (location.ResourceType == typeof(SceneInstance))
            {
                loadSceneBtn.style.display = DisplayStyle.Flex;
                icon.image = sceneIcon;
                if (EditorApplication.isPlaying)
                {
                    loadSceneBtn.clickable = new Clickable(() =>
                    {
                        var currentAddress = directoryContent.itemsSource[i] as string;
                        Addressables.LoadSceneAsync(currentAddress);
                    });
                }
                else
                {
                    loadSceneBtn.clickable = null;
                }
            }
        }
        private bool IsLoadableAsset(IResourceLocation location) =>
               location.ResourceType != typeof(SceneInstance)
            && location.ResourceType != typeof(IAssetBundleResource)
            && location.ProviderId != "UnityEngine.ResourceManagement.ResourceProviders.LegacyResourcesProvider";
        private async Task<Texture2D> RenderIcon(string address)
        {
            string previewCachePath = Path.Combine(PreviewRoot, $"{address}.png");
            if (File.Exists(previewCachePath))
            {
                var texture = new Texture2D(128, 128);
                texture.LoadImage(File.ReadAllBytes(previewCachePath));
                texture.Apply();
                PreviewCache[address] = texture;
                return texture;
            }

            Texture2D preview = null;
            Object result = null;
            try
            {
                result = await Addressables.LoadAssetAsync<Object>(address).Task;
                preview = UpdatePreview(result);
            }
            catch
            {
            }
            if (result)
                while (AssetPreview.IsLoadingAssetPreviews())
                {
                    await Task.Delay(100);
                    preview = UpdatePreview(result);
                    if (preview && preview.isReadable)
                    {
                        var png = preview.EncodeToPNG();
                        var fileName = $"{Path.GetFileName(address)}.png";
                        string addressFolder = Path.GetDirectoryName(address);
                        var finalFolder = Path.Combine(PreviewRoot, addressFolder);
                        Directory.CreateDirectory(finalFolder);
                        var filePath = Path.Combine(finalFolder, fileName);
                        File.WriteAllBytes(filePath, png);
                    }
                }

            return preview;
        }
        private Texture2D UpdatePreview(Object result)
        {
            Texture2D preview;
            switch (result)
            {
                case GameObject gobj when gobj.GetComponentsInChildren<SkinnedMeshRenderer>().Any()
                                       || gobj.GetComponentsInChildren<SpriteRenderer>().Any()
                                       || gobj.GetComponentsInChildren<MeshRenderer>().Any()
                                       || gobj.GetComponentsInChildren<CanvasRenderer>().Any():
                case Material _:
                    preview = AssetPreview.GetAssetPreview(result);
                    break;
                default:
                    preview = AssetPreview.GetMiniThumbnail(result);
                    break;
            }

            return preview;
        }
        private MethodInfo GetLoadMethod(IResourceLocation location)
        {
            var addressablesType = typeof(Addressables);
            var loadAsyncMethodTemplate = addressablesType.GetMethod(nameof(Addressables.LoadAssetAsync), loadParamTypes);
            var loadAssetAsync = loadAsyncMethodTemplate.MakeGenericMethod(location.ResourceType);
            return loadAssetAsync;
        }
        private void DirectoryContent_onSelectionChanged(List<object> obj)
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
        private VisualElement AssetLabel()
        {
            var element = new VisualElement { name = AddressableAssetName };
            element.Add(new Image { name = PreviewIcon });

            var labelContainer = new VisualElement { name = AddressableLabels };

            MakeReadonlyTextField(labelContainer, NameLabel);

            if (browserOptions.HasFlag(BrowserOptions.ShowType))
                MakeReadonlyTextField(labelContainer, TypeLabel);

            if (browserOptions.HasFlag(BrowserOptions.ShowProvider))
                MakeReadonlyTextField(labelContainer, ProviderLabel);

            element.Add(labelContainer);

            var buttonPanel = new VisualElement { name = ButtonPanel };

            var loadSceneBtn = new Button { name = LoadSceneButton, text = "Load Scene", tooltip = "Editor must be in Play mode to load Scenes" };
            buttonPanel.Add(loadSceneBtn);

            element.Add(buttonPanel);

            return element;
        }
    }
}
#endif
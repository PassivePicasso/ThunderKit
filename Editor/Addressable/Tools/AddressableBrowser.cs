#if TK_ADDRESSABLE
using System.Linq;
using ThunderKit.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThunderKit.Core.Windows;
using System.Text.RegularExpressions;
using UnityEngine.ResourceManagement;
using UnityEngine.AddressableAssets.ResourceLocators;
using System;
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
        private const string PreviewIcon = "addressable-icon";
        private const string AddressableAssetName = "addressable-asset";
        private const string ButtonPanel = "addressable-button-panel";
        private const string LoadSceneButton = "addressable-load-scene-button";
        private const string AddressableLabels = "addressable-labels";
        List<string> CatalogDirectories;
        Dictionary<string, List<string>> DirectoryContents;
        static Dictionary<string, Type> LocationType;

        private ListView directory;
        private ListView directoryContent;
        private Texture sceneIcon;
        private TextField searchBox;
        private EnumFlagsField regexOptionsField;
        private Toggle useRegexToggle;
        private Button helpButton;
        public bool useRegex;
        public bool caseSensitive;
        public string searchInput;
        public RegexOptions regexOptions;
        private Regex regex;
        private string typeFilter;

        private static void LoadIndex()
        {
            if (!Addressables.ResourceLocators.Any()) return;

            LocationType = new Dictionary<string, Type>();
            foreach (var locator in Addressables.ResourceLocators)
            {
                switch (locator)
                {
                    case ResourceLocationMap rlm:
                        foreach (var location in rlm.Locations.SelectMany(loc => loc.Value))
                            LocationType[location.PrimaryKey] = location.ResourceType;
                        break;
                }
            }
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
            regexOptionsField = rootVisualElement.Q<EnumFlagsField>("regex-options");
            directory = rootVisualElement.Q<ListView>("directory");
            directoryContent = rootVisualElement.Q<ListView>("directory-content");
            useRegexToggle = rootVisualElement.Q<Toggle>("use-regex-toggle");
            helpButton = rootVisualElement.Q<Button>("help-button");
            helpButton.clickable = new Clickable(() =>
            {
                Documentation.ShowThunderKitDocumentation("Packages/com.passivepicasso.thunderkit/Documentation/ThunderKitDocumentation/Manual/AddressableBrowser.md");
            });

            directory.makeItem = DirectoryLabel;
            directoryContent.makeItem = AssetLabel;

            directory.bindItem = (element, i) =>
            {
                Label label = element.Q<Label>(NameLabel);
                var address = (string)directory.itemsSource[i];
                label.text = address;
            };
            directoryContent.bindItem = BindAsset;

            LoadIndex();
            var allKeys = Addressables.ResourceLocators.SelectMany(locator => locator.Keys).Select(key => key.ToString());

            var allGroups = allKeys.GroupBy(key => Path.GetDirectoryName(key).Replace("\\", "/"));
            DirectoryContents = allGroups.ToDictionary(g => g.Key, g => g.OrderBy(k => k).ToList());
            CatalogDirectories = DirectoryContents.Keys.OrderBy(k => k).ToList();
            var scenes = allKeys.Where(key => key.EndsWith(".unity")).ToList();
            CatalogDirectories.Insert(0, "Scenes");
            DirectoryContents["Scenes"] = scenes;

            directory.itemsSource = CatalogDirectories;

            directory.Refresh();
            directory.onSelectionChanged += Directory_onSelectionChanged;
            directoryContent.onSelectionChanged += DirectoryContent_onSelectionChanged;
#if UNITY_2019_1_OR_NEWER
            searchBox.RegisterValueChangedCallback(OnSearchChanged);
            regexOptionsField.RegisterValueChangedCallback(OnRegexOptionsChanged);
            useRegexToggle.RegisterValueChangedCallback(OnUseRegexChanged);
#else
            searchBox.OnValueChanged(OnSearchChanged);
            regexOptionsField.OnValueChanged(OnRegexOptionsChanged);
#endif
            RefreshSearch();
        }

        private void OnUseRegexChanged(ChangeEvent<bool> evt) => RefreshSearch();
        private void OnRegexOptionsChanged(ChangeEvent<System.Enum> evt) => RefreshSearch();
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

            if (!useRegex) searchValue = Regex.Escape(searchValue);

            if (string.IsNullOrEmpty(searchValue))
                regex = null;
            else
                regex = new Regex(searchValue, regexOptions);

            if (noFilter)
                directory.itemsSource = CatalogDirectories;
            else
                EditorApplication.update += Refresh;
        }

        private void Refresh()
        {
            EditorApplication.update -= Refresh;
            if (regex != null || !string.IsNullOrEmpty(typeFilter))
            {
                var list = new List<string>();

                foreach (var kvp in DirectoryContents)
                    Filter(list, kvp.Value, true, location => Path.GetDirectoryName(location).Replace("\\", "/"));

                directory.itemsSource = list;
            }
            else directory.itemsSource = DirectoryContents.Keys.ToList();
        }

        private void Directory_onSelectionChanged(List<object> obj)
        {
            var selected = obj.First().ToString();
            var addresses = DirectoryContents[selected];

            if (regex != null || !string.IsNullOrEmpty(typeFilter))
            {
                var list = new List<string>();
                Filter(list, addresses, false, location => location);
                directoryContent.itemsSource = list;
            }
            else
                directoryContent.itemsSource = addresses;
        }
        private void Filter(List<string> list, List<string> values, bool earlyReturn, Func<string, string> processValue)
        {
            foreach (var location in values)
            {
                if (regex != null && !regex.IsMatch(location)) continue;
                if (!string.IsNullOrEmpty(typeFilter))
                    if (!LocationType.ContainsKey(location)) continue;
                    else if (!LocationType[location].FullName.Contains(typeFilter)) continue;
                list.Add(processValue(location));
                if (earlyReturn)
                    break;
            }
        }


        async void BindAsset(VisualElement element, int i)
        {
            var icon = element.Q<Image>(PreviewIcon);
            var nameLabel = element.Q<Label>(NameLabel);
            var typeLabel = element.Q<Label>(TypeLabel);
            var copyBtn = element.Q<Button>(CopyButton);
            var loadSceneBtn = element.Q<Button>(LoadSceneButton);
            copyBtn.clickable = new Clickable(() =>
            {
                var text = directoryContent.itemsSource[i] as string;
                EditorGUIUtility.systemCopyBuffer = text;
            });
            var address = (string)directoryContent.itemsSource[i];
            nameLabel.text = address;
            string typeText = string.Empty;
            if (LocationType.ContainsKey(address))
                typeText = LocationType[address].FullName;
            typeLabel.text = typeText;

            icon.image = null;
            if (!address.EndsWith(".unity"))
            {
                var texture = await RenderIcon(address);
                if (texture)
                {
                    icon.image = texture;
                    Repaint();
                }
            }
            else
                icon.image = sceneIcon;

            if (address.EndsWith(".unity") && EditorApplication.isPlaying)
            {
                loadSceneBtn.RemoveFromClassList("hidden");
                loadSceneBtn.clickable = new Clickable(() =>
                {
                    var currentAddress = directoryContent.itemsSource[i] as string;
                    Addressables.LoadSceneAsync(currentAddress);
                });
            }
            else
                loadSceneBtn.AddToClassList("hidden");
        }
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
                if (!address.EndsWith(".unity"))
                {
                    var loadOperation = Addressables.LoadAssetAsync<Object>(address);
                    await loadOperation.Task;
                    result = loadOperation.Result;
                    preview = UpdatePreview(result);
                }
            }
            catch { }
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

        private static Texture2D UpdatePreview(Object result)
        {
            Texture2D preview;
            switch (result)
            {
                case GameObject gobj when gobj.GetComponentsInChildren<SkinnedMeshRenderer>().Any()
                                       || gobj.GetComponentsInChildren<SpriteRenderer>().Any()
                                       || gobj.GetComponentsInChildren<MeshRenderer>().Any()
                                       || gobj.GetComponentsInChildren<CanvasRenderer>().Any():
                case Material mat:
                    preview = AssetPreview.GetAssetPreview(result);
                    break;
                default:
                    preview = AssetPreview.GetMiniThumbnail(result);
                    break;
            }

            return preview;
        }
        private void DirectoryContent_onSelectionChanged(List<object> obj)
        {
            try
            {
                var first = obj.OfType<string>().First();
                if (first.EndsWith(".unity")) return;
                var firstOp = Addressables.LoadAssetAsync<Object>(first);
                var firstObj = firstOp.WaitForCompletion();
                firstObj.hideFlags |= HideFlags.NotEditable;
                Selection.activeObject = firstObj;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        VisualElement DirectoryLabel() => new Label { name = NameLabel };
        VisualElement AssetLabel()
        {
            var element = new VisualElement { name = AddressableAssetName };
            element.Add(new Image { name = PreviewIcon });
            var labelContainer = new VisualElement { name = AddressableLabels };
            labelContainer.Add(new Label { name = NameLabel });
            labelContainer.Add(new Label { name = TypeLabel });
            element.Add(labelContainer);

            var buttonPanel = new VisualElement { name = ButtonPanel };
            buttonPanel.Add(new Button { name = CopyButton, text = "Copy Address" });

            var loadSceneBtn = new Button { name = LoadSceneButton, text = "Load Scene" };
            loadSceneBtn.AddToClassList("hidden");

            buttonPanel.Add(loadSceneBtn);
            element.Add(buttonPanel);

            return element;
        }
    }
}
#endif
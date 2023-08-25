using ThunderKit.Common;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ThunderKit.Markdown.ObjectRenderers;
using ThunderKit.Markdown;
using ThunderKit.Core.Data;
using System;
using UnityEngine;
using UnityEditorInternal;
using ThunderKit.Markdown.Extensions.Json;
using ThunderKit.Core.Documentation;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Windows
{
    public enum MarkdownOpenMode { OperatingSystemDefault, UnityExternalEditor }
    public class Documentation : TemplatedWindow
    {
        private const string PageClass = "pagelistview__page";
        private const string PageHeaderClass = "pagelistview__page--header";
        private const string ElementClass = "pagelistview__item";
        private const string SelectedClass = "selected";
        private const string HiddenClass = "hidden";
        private const string HideArrowClass = "hide-arrow";
        private const string MinimizeClass = "minimize";
        private const string RootDocumentationElementName = "documentation-markdown";

        public static bool IsOpen { get; private set; }

        private string currentPage;

        [InitializeOnLoadMethod]
        static void InitializeDocumentation()
        {
            LinkInlineRenderer.RegisterScheme(
                "documentation",
                link =>
                {
                    var schemelessUri = link.Substring("documentation://".Length);

                    if (schemelessUri.Length == 0) return;

                    string path = schemelessUri.StartsWith("GUID/") ?
                    AssetDatabase.GUIDToAssetPath(schemelessUri.Substring("GUID/".Length))
                    : schemelessUri;

                    ShowThunderKitDocumentation(path);
                },
                label =>
                {
                    string schemelessUri = label.tooltip.Substring("documentation://".Length);

                    if (schemelessUri.Length != 0)
                    {
                        string path = schemelessUri.StartsWith("GUID/") ?
                        AssetDatabase.GUIDToAssetPath(schemelessUri.Substring("GUID/".Length))
                        : schemelessUri;
                        label.tooltip = $"documentation://{path}";
                    }

                    var container = new VisualElement();

                    var icon = new Image();
                    icon.AddToClassList("asset-icon");
                    icon.image = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("2f1c3b93b0c7f4046a4d826cc0460f12"));

                    container.Add(icon);
                    container.Add(label);

                    return container;
                });
        }

        internal static Documentation instance;
        private ScrollView indexScroller;
        private MarkdownElement markdownElement;
        private VisualElement pageList;
        private EventHandler<(string, MarkdownFileWatcher.ChangeType)> documentUpdated;
        public static Documentation ShowDocumentation()
        {
            if (!IsOpen || instance == null)
            {
                var consoleType = typeof(EditorWindow).Assembly.GetTypes()
                    .Where(t => t.Name.Contains("SceneView")
                             || t.Name.Contains("Game")
                             || t.Name.Contains("ProjectSettings")
                             || t.Name.Contains("Store"));

                instance = GetWindow<Documentation>("Documentation", consoleType.ToArray());
            }
            instance.Focus();

            return instance;
        }

        private void MarkdownFileWatcher_DocumentUpdated(object sender, (string path, MarkdownFileWatcher.ChangeType change) e)
        {
            Initialize();
            switch (e.change)
            {
                case MarkdownFileWatcher.ChangeType.Imported:
                case MarkdownFileWatcher.ChangeType.Moved:
                    LoadSelection(e.path);
                    break;
                case MarkdownFileWatcher.ChangeType.Deleted:
                    if (currentPage == e.path) currentPage = null;
                    LoadSelection(Constants.ThunderKitRoot + "/Documentation/topics/1st Read Me!.md");
                    break;
            }
        }

        [MenuItem(Constants.ThunderKitMenuRoot + "Documentation")]
        public static void ShowThunderKitDocumentation()
        {
            ShowDocumentation();
        }

        public static void ShowThunderKitDocumentation(string pagePath)
        {
            ShowDocumentation().LoadSelection(pagePath);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            instance = this;
            IsOpen = true;
            Initialize();
        }
        private void Initialize()
        {
            if (documentUpdated == null)
            {
                documentUpdated = new EventHandler<(string, MarkdownFileWatcher.ChangeType)>(MarkdownFileWatcher_DocumentUpdated);
                MarkdownFileWatcher.DocumentUpdated += documentUpdated;
            }

            var documentationRoots = AssetDatabase.FindAssets($"t:{nameof(DocumentationRoot)}")
                    .Distinct()
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Distinct()
                    .Select(path => (path: Path.GetDirectoryName(path).Replace("\\", "/"), asset: AssetDatabase.LoadAssetAtPath<DocumentationRoot>(path)))
                    .ToArray();

            indexScroller = rootVisualElement.Q<ScrollView>("index-scroller");
            rootVisualElement.AddManipulator(new MarkdownContextualMenuManipulator());

            markdownElement = rootVisualElement.Q<MarkdownElement>(RootDocumentationElementName);

            pageList = rootVisualElement.Q("page-list");
            pageList.RegisterCallback<KeyDownEvent>(OnNavigate);
            pageList.Clear();

            var rootArray = new string[1];
            foreach (var root in documentationRoots)
            {
                rootArray[0] = root.path;
                var topicsFileGuids = AssetDatabase.FindAssets($"t:TextAsset", rootArray);
                var topicsFilePaths = topicsFileGuids.Select(AssetDatabase.GUIDToAssetPath).Where(path => Path.GetExtension(path).Equals(".md")).ToArray();
                var uxmlTopics = topicsFilePaths.Distinct().ToArray();
                var pageFiles = uxmlTopics
                    .OrderBy(dir => Path.GetDirectoryName(dir))
                    .ThenBy(path => Path.GetFileNameWithoutExtension(path))
                    .ToArray();

                var pages = new Dictionary<string, PageEntry>();
                if (root.asset.MainPage == null)
                {
                    Debug.LogWarning($"Documentation Root: {root.asset.name}, has not been assigned a MainPain, skipping.", root.asset);
                    continue;
                }
                var mainPagePath = AssetDatabase.GetAssetPath(root.asset.MainPage);
                var rootPage = new PageEntry(root.asset.name, root.asset.MainPage.name, mainPagePath, OnSelect);
                pageList.Add(rootPage);
                foreach (var pagePath in pageFiles)
                {
                    if (pagePath == mainPagePath) continue;
                    var pageName = Path.GetFileNameWithoutExtension(pagePath);
                    var containingDirectory = Path.GetDirectoryName(pagePath);
                    var pageNamePath = Path.Combine(containingDirectory, pageName);

                    var fullPageName = GetPageName(pageNamePath);
                    var parentPage = pages.TryGetValue(containingDirectory, out var tempPage) ? tempPage : null;

                    var pageEntry = new PageEntry(pageName, pageName, pagePath, OnSelect);

                    pageEntry.RegisterCallback<KeyDownEvent>(OnNavigate);

                    if (parentPage != null) parentPage.AddChildPage(pageEntry);
                    else
                        rootPage.AddChildPage(pageEntry);
                    pages.Add(pageNamePath, pageEntry);
                }
            }
            LoadSelection(Constants.ThunderKitRoot + "/Documentation/ThunderKitDocumentation/About.md");
        }


        string GetPageName(string path)
        {
            var cleaned = path.Replace("\\", "-").Replace("/", "-").Replace(" ", "_").ToLower();
            cleaned = cleaned.Substring(cleaned.LastIndexOf("documentation-") + "documentation-".Length);
            return cleaned;

        }

        private void OnNavigate(KeyDownEvent evt)
        {
            var pageList = rootVisualElement.Q("page-list");
            var pageIndex = pageList.Query<PageEntry>().Build().ToList()
                .Where(e =>
                {
                    var current = e;
                    if (current.parentEntry == null) return true;
                    current = current.parentEntry;
                    while (current != null)
                    {
                        if (!current.value) return false;
                        current = current.parentEntry;
                    }
                    return true;
                }).ToList();
            var selectedPage = pageList.Query<PageEntry>(className: SelectedClass).Build().First();
            var selectedIndex = pageIndex.IndexOf(selectedPage);
            int modifier = 0;

            switch (evt.keyCode)
            {
                case UnityEngine.KeyCode.UpArrow:
                    if (selectedIndex > 0)
                        modifier = -1;
                    break;

                case UnityEngine.KeyCode.DownArrow:
                    if (selectedIndex < pageIndex.Count)
                        modifier = 1;
                    break;

                case UnityEngine.KeyCode.LeftArrow:
                    if (selectedPage.childEntries.Length > 0 && selectedPage.value)
                        selectedPage.Toggle(false);
                    else
                        for (modifier = -1; selectedIndex + modifier > 0; modifier--)
                        {
                            if (pageIndex[selectedIndex + modifier].childEntries.Length > 0
                                && pageIndex[modifier + selectedIndex].value)
                                break;
                        }
                    break;

                case UnityEngine.KeyCode.RightArrow:
                    if (!selectedPage.value)
                        selectedPage.Toggle(true);
                    else if (selectedPage.childEntries.Length > 0)
                    {
                        for (modifier = 1; selectedIndex + modifier < pageIndex.Count; modifier++)
                        {
                            if (pageIndex[modifier + selectedIndex].childEntries.Length > 0
                                && !pageIndex[modifier + selectedIndex].value)
                                break;
                        }
                    }
                    break;
            }
            if (modifier != 0)
            {
                int newSelectedIndex = selectedIndex + modifier;
                if (newSelectedIndex > -1 && newSelectedIndex < pageIndex.Count)
                {
                    var newSelectedPage = pageIndex[newSelectedIndex];
                    UpdateSelectedPage(selectedPage, newSelectedPage);
#if UNITY_2021_1_OR_NEWER
                    if(indexScroller.verticalScrollerVisibility != ScrollerVisibility.Hidden)
#else
                    if (indexScroller.showVertical)
#endif
                    {
                        var dist = (float)(newSelectedIndex + modifier) / pageIndex.Count;
                        float highValue = indexScroller.verticalScroller.highValue * 1.25f;
                        var newValue = (highValue * dist) - (highValue * 0.125f);
                        indexScroller.verticalScroller.value = newValue;
                    }
                }
            }

        }

        private void UpdateSelectedPage(PageEntry selectedPage, PageEntry newSelectedPage)
        {
            selectedPage.RemoveFromClassList(SelectedClass);
            newSelectedPage.AddToClassList(SelectedClass);
            LoadSelection(newSelectedPage);
        }

        void OnSelect(EventBase e)
        {
            var element = e.currentTarget as VisualElement;

            while (element != null && !(element is PageEntry))
                element = element.parent;

            var entry = element as PageEntry;
            entry.Toggle(true);

            if (element != null)
                LoadSelection(entry);
        }

        public void LoadSelection(string pagePath)
        {
            var pageEntryQuery = rootVisualElement.Query<PageEntry>().ToList();
            var entry = pageEntryQuery.FirstOrDefault(pe => pe.PagePath.Equals(pagePath, System.StringComparison.OrdinalIgnoreCase));
            if (entry != null)
                LoadSelection(entry);
        }

        private void LoadSelection(PageEntry element)
        {
            var pageList = rootVisualElement.Q("page-list");
            var selectedElement = pageList.Q(className: SelectedClass);
            selectedElement?.RemoveFromClassList(SelectedClass);
            element.AddToClassList(SelectedClass);

            currentPage = element.PagePath;

            var parent = element.parentEntry;
            while (parent != null)
            {
                if (!parent.value)
                    parent.Toggle(true);

                parent = parent.parentEntry;
            }

            markdownElement.Data = element.PagePath;
            markdownElement.RefreshContent();

        }


        public class PageEntry : VisualElement, INotifyValueChanged<bool>
        {
            public Label Label { get; private set; }
            internal VisualElement container;
            private VisualElement ArrowIcon;

            public PageEntry parentEntry { get; private set; }
            public PageEntry[] childEntries => container.Children().OfType<PageEntry>().ToArray();

            bool toggled = false;
            public bool value
            {
                get => toggled; set
                {
                    var oldValue = toggled;
                    toggled = value;
                    valueChanged?.Invoke(ChangeEvent<bool>.GetPooled(oldValue, value));
                }
            }

            event EventCallback<ChangeEvent<bool>> valueChanged;

            public readonly string PagePath;

            public PageEntry(string pageName, string name, string pagePath, Action<EventBase> onSelect)
            {
                this.name = name;
                PagePath = pagePath;

                container = new VisualElement();
                container.name = "page-entry-children";

                Label = new Label
                {
                    text = ObjectNames.NicifyVariableName(pageName)
                };
                Label.AddToClassList(PageClass);

                var header = new VisualElement();
                header.AddToClassList("header");
                ArrowIcon = new VisualElement();

                header.Add(ArrowIcon);
                ArrowIcon.AddToClassList("in-foldout");
                header.Add(Label);
                Add(header);
                Add(container);
                container.AddToClassList(HiddenClass);
                container.AddToClassList(MinimizeClass);
                ArrowIcon.AddToClassList(HiddenClass);

                AddToClassList(PageHeaderClass);
                AddToClassList(ElementClass);
                Label.AddManipulator(new Clickable(onSelect));
                ArrowIcon.AddManipulator(new Clickable(() => Toggle(!value)));
                SetValueWithoutNotify(false);
            }
            public void AddChildPage(PageEntry child)
            {
                child.parentEntry = this;
                container.Add(child);
                ArrowIcon.RemoveFromClassList(HiddenClass);
            }

            public void Toggle(bool state)
            {
                value = state;
                if (value)
                {
                    ArrowIcon.RemoveFromClassList("in-foldout");
                    ArrowIcon.AddToClassList("in-foldout-on");
                    container.RemoveFromClassList(HiddenClass);
                    container.RemoveFromClassList(MinimizeClass);
                }
                else
                {
                    ArrowIcon.AddToClassList("in-foldout");
                    ArrowIcon.RemoveFromClassList("in-foldout-on");
                    container.AddToClassList(HiddenClass);
                    container.AddToClassList(MinimizeClass);
                    if (ClassListContains(SelectedClass))
                    {
                        instance.LoadSelection(PagePath);
                    }
                }
            }

            public void RemoveOnValueChanged(EventCallback<ChangeEvent<bool>> eventCallback)
            {
                valueChanged -= eventCallback;
            }
            public void OnValueChanged(EventCallback<ChangeEvent<bool>> eventCallback)
            {
                valueChanged += eventCallback;
            }

            public void SetValueAndNotify(bool newValue)
            {
                value = newValue;
            }
            public void SetValueWithoutNotify(bool newValue)
            {
                toggled = newValue;
            }
        }
    }
}
using ThunderKit.Common;
using ThunderKit.Core.UIElements;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ThunderKit.Markdown.ObjectRenderers;
using ThunderKit.Markdown;
using ThunderKit.Core.Data;
using System;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Windows
{
    public class Documentation : TemplatedWindow
    {
        private const string PageClass = "pagelistview__page";
        private const string PageHeaderClass = "pagelistview__page--header";
        private const string ElementClass = "pagelistview__item";
        private const string SelectedClass = "selected";
        private const string HiddenClass = "hidden";
        private const string HideArrowClass = "hide-arrow";
        private const string MinimizeClass = "minimize";

        [InitializeOnLoadMethod]
        static void InitializeDocumentation()
        {
            LinkInlineRenderer.RegisterScheme(
                "documentation",
                link =>
                {
                    var path = link.Substring("documentation://".Length);
                    ShowThunderKitDocumentation(path);
                });
        }

        [MenuItem(Constants.ThunderKitMenuRoot + "Documentation")]
        public static void ShowThunderKitDocumentation() => GetWindow<Documentation>();
        public static void ShowThunderKitDocumentation(string pagePath)
        {
            var documentationWindow = GetWindow<Documentation>();
            documentationWindow.LoadSelection(pagePath);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var documentationRoots = AssetDatabase.FindAssets($"t:{nameof(DocumentationRoot)}")
                    .Distinct()
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Distinct()
                    .Select(path => (path: Path.GetDirectoryName(path), asset: AssetDatabase.LoadAssetAtPath<DocumentationRoot>(path)))
                    .Select(p => (path: p.path.Replace("\\", "/"), asset: p.asset))
                    .ToArray();

            var pageList = rootVisualElement.Q("page-list");
            var topicsFileGuids = AssetDatabase.FindAssets($"t:TextAsset", documentationRoots.Select(r => r.path).ToArray());
            var topicsFilePaths = topicsFileGuids.Select(AssetDatabase.GUIDToAssetPath).Where(path => Path.GetExtension(path).Equals(".md")).ToArray();
            var uxmlTopics = topicsFilePaths.Distinct().ToArray();
            var pageFiles = uxmlTopics
                .OrderBy(dir => Path.GetDirectoryName(dir))
                .ThenBy(path => Path.GetFileNameWithoutExtension(path))
                .ToArray();
            pageList.RegisterCallback<KeyDownEvent>(OnNavigate);
            pageList.Clear();

            var pages = new Dictionary<string, PageEntry>();
            PageEntry defaultPage = null;
            foreach (var pagePath in pageFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(pagePath);
                var containingDirectory = Path.GetDirectoryName(pagePath);
                var pageNamePath = Path.Combine(containingDirectory, fileName);

                var fullParentPageName = GetPageName(containingDirectory);
                var fullPageName = GetPageName(pageNamePath);
                var parentPage = pages.TryGetValue(fullParentPageName, out var tempPage) ? tempPage : null;

                var pageEntry = new PageEntry(fileName, fullPageName, pagePath, OnSelect);

                if (fullPageName.Equals("topics-1st_read_me!"))
                {
                    defaultPage = pageEntry;
                }
                if (parentPage != null) parentPage.AddChildPage(pageEntry);
                else
                    pageList.Add(pageEntry);


#if UNITY_2019_1_OR_NEWER
                rootVisualElement.RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
#endif
                pages.Add(fullPageName, pageEntry);
            }
            if (defaultPage != null)
                LoadSelection(defaultPage);
        }
#if UNITY_2019_1_OR_NEWER
        private void OnStyleResolved(CustomStyleResolvedEvent evt)
        {
            var root = evt.currentTarget as VisualElement;
            foreach (var pageEntry in root.Query<PageEntry>().Build().ToList())
            {
                float left = pageEntry.Depth * 12;
                pageEntry.style.paddingLeft = new StyleLength(new Length(left, LengthUnit.Pixel));
            }

        }
#endif


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
                    modifier = -1;
                    break;

                case UnityEngine.KeyCode.DownArrow:
                    modifier = 1;
                    break;

                case UnityEngine.KeyCode.LeftArrow:
                    if (selectedPage.childEntries.Length > 0 && selectedPage.value)
                        selectedPage.Toggle(false);
                    else if (selectedPage.parentEntry != null)
                        UpdateSelectedPage(pageIndex, selectedPage, selectedPage.parentEntry);
                    break;

                case UnityEngine.KeyCode.RightArrow:
                    if (!selectedPage.value)
                        selectedPage.Toggle(true);
                    else if (selectedPage.childEntries.Length > 0)
                        modifier = 1;
                    break;
            }
            if (modifier != 0)
            {
                int newSelectedIndex = selectedIndex + modifier;
                if (newSelectedIndex > -1 && newSelectedIndex < pageIndex.Count)
                {
                    var newSelectedPage = pageIndex[newSelectedIndex];
                    UpdateSelectedPage(pageIndex, selectedPage, newSelectedPage);
                }
            }
        }

        private void UpdateSelectedPage(List<PageEntry> pageIndex, PageEntry selectedPage, PageEntry newSelectedPage)
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
            var entry = pageEntryQuery.First(pe => pe.PagePath.Equals(pagePath, System.StringComparison.OrdinalIgnoreCase));
            LoadSelection(entry);
        }

        private void LoadSelection(PageEntry element)
        {
            var pageList = rootVisualElement.Q("page-list");
            var selectedElement = pageList.Q(className: SelectedClass);
            selectedElement?.RemoveFromClassList(SelectedClass);
            element.AddToClassList(SelectedClass);

            var markdownElement = rootVisualElement.Q<MarkdownElement>("documentation-markdown");
            markdownElement.Data = element.PagePath;
            markdownElement.RefreshContent();
        }


        public class PageEntry : BaseField<bool>
        {
            private Label Label;
            internal VisualElement container;
            private VisualElement ArrowIcon;

            public PageEntry parentEntry { get; private set; }
            public PageEntry[] childEntries => container.OfType<PageEntry>().ToArray();

            public readonly string PagePath;

            public PageEntry(string templateName, string name, string pagePath, Action<EventBase> onSelect)
            {
                this.name = name;
                PagePath = pagePath;

                container = new VisualElement();
                container.name = "page-entry-children";

                Label = new Label
                {
                    text = ObjectNames.NicifyVariableName(templateName)
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
                        var documentationWindow = GetWindow<Documentation>();
                        documentationWindow.LoadSelection(PagePath);
                    }
                }
            }
        }
    }
}
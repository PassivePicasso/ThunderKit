using System.Collections;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
using ThunderKit.Core.Data;
using ThunderKit.Markdown;
using UnityEngine;
using UnityEditor.Callbacks;
using ThunderKit.Common;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Windows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;
    public class PipelineLogWindow : TemplatedWindow
    {
        private static PipelineLogWindow window;
        public static bool IsOpen { get; private set; }

        private ThunderKitSettings settings;
        private ListView logEntryListView;
        private bool locked = false;
        private PipelineLog pipelineLog;
        private Label nameLabel;
        private Label createdDateLabel;
        private List<LogLevel> levelFilters = new List<LogLevel> { LogLevel.Information, LogLevel.Warning, LogLevel.Error };

        public static void ShowLog(PipelineLog pipelineLog)
        {
            if (window == null || !IsOpen)
            {
                var consoleType = typeof(EditorWindow).Assembly.GetTypes().First(t => "ConsoleWindow".Equals(t.Name));
                window = GetWindow<PipelineLogWindow>($"{pipelineLog.pipeline?.name ?? pipelineLog.name}", consoleType);
            }
            window.settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            window.pipelineLog = pipelineLog;
            window.Initialize();
        }

        public static void Update(PipelineLog pipelineLog)
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            if (((window == null || !IsOpen)) && settings?.ShowLogWindow == true)
                ShowLog(pipelineLog);
            else if (settings.ShowLogWindow == true)
            {
                window.nameLabel.text = pipelineLog.name;
                window.createdDateLabel.text = pipelineLog.CreatedDate.ToString(window.settings.CreatedDateFormat);
                window.logEntryListView.itemsSource = (IList)pipelineLog.Entries.Where(entry => window.levelFilters.Contains(entry.logLevel)).ToList();
                if (window.logEntryListView.itemsSource.Count > 0)
                    window.logEntryListView.selectedIndex = 0;
#if UNITY_2021_2_OR_NEWER
                window.logEntryListView.Rebuild();
#else
                window.logEntryListView.Refresh();
#endif
                if (settings.ShowLogWindow)
                {
                    window.Repaint();
                }
            }
        }


        [OnOpenAsset]
        public static bool OnOpen(int instanceId, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset is PipelineLog log)
            {
                ShowLog(log);
                return true;
            }
            return false;
        }

        private void OnSelectionChange()
        {
            if (locked) return;
            if (Selection.objects.Length > 1) { }
            if (Selection.activeObject is PipelineLog pipelineLog)
            {
                ShowLog(pipelineLog);
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Initialize();

            IsOpen = true;
        }
        private void OnDestroy() => IsOpen = false;
        private void OnDisable() => IsOpen = false;

        private void Initialize()
        {
            if (!pipelineLog) return;
            var content = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
            content.text = $"Pipeline Log";
            titleContent = content;
            if (logEntryListView == null)
            {
                logEntryListView = rootVisualElement.Q<ListView>("logentry-list-view");
                logEntryListView.bindItem = OnBind;
                logEntryListView.makeItem = OnMake;
#if UNITY_2020_1_OR_NEWER
                logEntryListView.onSelectionChange += UpdateContextWindowSelect;
                logEntryListView.onItemsChosen += UpdateContextWindow;
#else
                logEntryListView.onSelectionChanged += UpdateContextWindowSelect;
                logEntryListView.onItemChosen += UpdateContextWindow;
#endif
            }
            nameLabel = rootVisualElement.Q<Label>("name-label");
            createdDateLabel = rootVisualElement.Q<Label>("created-date-label");

            nameLabel.text = pipelineLog.name;
            createdDateLabel.text = pipelineLog.CreatedDate.ToString(settings.CreatedDateFormat);
            RefreshListView();

            ConfigureFilterButton(LogLevel.Error, rootVisualElement.Q<Button>("error-filter-button"));
            ConfigureFilterButton(LogLevel.Information, rootVisualElement.Q<Button>("information-filter-button"));
            ConfigureFilterButton(LogLevel.Verbose, rootVisualElement.Q<Button>("verbose-filter-button"));
            ConfigureFilterButton(LogLevel.Warning, rootVisualElement.Q<Button>("warning-filter-button"));

            rootVisualElement.AddSheet(Constants.ThunderKitStyle);
            rootVisualElement.Bind(new SerializedObject(this));
        }

        string FilterStateMessage(LogLevel level) => level switch
        {
            LogLevel.Information => $"{level} {(levelFilters.Contains(level) ? "Visible" : "Hidden")}",
            LogLevel.Verbose => $"{level} {(levelFilters.Contains(level) ? "Visible" : "Hidden")}",
            _=> $"{level}s {(levelFilters.Contains(level) ? "Visible" : "Hidden")}"
        };

        void ConfigureFilterButton(LogLevel level, Button button)
        {
            var filterActive = levelFilters.Contains(level);
            button.tooltip = FilterStateMessage(level);
            button.clickable.clicked += () =>
            {
                if (levelFilters.Contains(level)) levelFilters.Remove(level);
                else levelFilters.Add(level);
                var updatedActive = levelFilters.Contains(level);
                button.tooltip = FilterStateMessage(level);
                button.ToggleInClassList("filter-active");
                RefreshListView();
            };
            if (!levelFilters.Contains(level)) button.RemoveFromClassList("filter-active");
            if (levelFilters.Contains(level)) button.AddToClassList("filter-active");
        }

        private void RefreshListView()
        {
            logEntryListView.itemsSource = (IList)pipelineLog.Entries.Where(entry => levelFilters.Contains(entry.logLevel)).ToList();
            if (logEntryListView.itemsSource.Count > 0)
                logEntryListView.selectedIndex = 0;
        }

#if UNITY_2020_1_OR_NEWER
        private void UpdateContextWindow(IEnumerable<object> obj) => LogContextWindow.ShowContext(obj.OfType<LogEntry>().First());
#else
        private void UpdateContextWindow(object obj)
        {
            LogEntry entry = (LogEntry)obj;
            if (entry.context != null && entry.context.Length > 0)
                LogContextWindow.ShowContext(entry);
        }
#endif
        private void UpdateContextWindowSelect(
#if UNITY_2020_1_OR_NEWER
            IEnumerable<object> obj
#else
            List<object> obj
#endif
            )
        {
            if (LogContextWindow.instance)
                LogContextWindow.ShowContext(obj.OfType<LogEntry>().First());
        }

        protected virtual void ShowButton(Rect r)
        {
            GUIStyle lockButton = new GUIStyle("IN LockButton");
            locked = GUI.Toggle(r, locked, new GUIContent(), lockButton);
        }

        void OnBind(VisualElement element, int entryIndex)
        {
            var entry = (LogEntry)logEntryListView.itemsSource[entryIndex];
            var timeStamp = element.Q<Label>("time-stamp");
            var shotContextButton = element.Q<Button>("show-context-button");
#if UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
#endif

            var icon = element.Q<VisualElement>("icon-log-level");
            var messageElement = element.Q<MarkdownElement>("message-label");

            foreach (var value in Enum.GetValues(typeof(LogLevel)).OfType<LogLevel>())
            {
                icon.EnableInClassList(IconClass(value), false);
                element.EnableInClassList(LevelClass(value), false);
            }
            var entryLevelIconClass = IconClass(entry.logLevel);
            var entryLevelClass = LevelClass(entry.logLevel);

            icon.EnableInClassList(entryLevelIconClass, true);
            element.EnableInClassList(entryLevelClass, true);

            messageElement.Data = entry.message;
            messageElement.RefreshContent();
            timeStamp.text = entry.time.ToString(settings.DateTimeFormat);
            element.userData = entry.context;
            if (entry.context != null && entry.context.Length > 0)
            {
                shotContextButton.RemoveFromClassList("hidden");

                void Update()
                {
#if UNITY_2020_1_OR_NEWER
                    UpdateContextWindow(new object[] { entry });
#else
                    UpdateContextWindow(entry);
#endif
                }
#if UNITY_2019_1_OR_NEWER
                shotContextButton.clickable = new Clickable(Update);
#elif UNITY_2018_1_OR_NEWER
                shotContextButton.clickable.clicked += Update;
#endif
            }
            else
            {
                shotContextButton.AddToClassList("hidden");
                shotContextButton.clickable = new Clickable(() => { });

            }
        }
        string LevelClass(LogLevel value) => $"{value}".ToLower();

        string IconClass(LogLevel logLevel) => $"{LevelClass(logLevel)}-icon";

        VisualElement OnMake() => GetTemplateInstance("LogEntryView").Children().First();
    }
}
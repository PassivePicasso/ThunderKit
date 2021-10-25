using System.Collections;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
using ThunderKit.Core.Data;
using ThunderKit.Markdown;
using UnityEngine;
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
        public Pipeline focusedPipeline;
        private PipelineSettings settings;
        public string pipelineName;
        private ListView logEntryListView;
        private Button clearLogButton;
        private bool locked = false;


        public static void ShowLog(Pipeline pipeline)
        {
            var consoleType = typeof(EditorWindow).Assembly.GetTypes().First(t => "ConsoleWindow".Equals(t.Name));
            var window = GetWindow<PipelineLogWindow>($"{pipeline.name} Log", consoleType);
            window.focusedPipeline = pipeline;
            window.settings = ThunderKitSetting.GetOrCreateSettings<PipelineSettings>();
            window.Initialize();
        }

        private void OnSelectionChange()
        {
            if (locked) return;
            if (Selection.objects.Length > 1) { }
            if (Selection.activeObject is Pipeline pipeline)
            {
                focusedPipeline.LogUpdated -= Pipeline_LogUpdated;
                focusedPipeline = pipeline;
                Initialize();
            }
            else if (logEntryListView != null)
            {
                logEntryListView.itemsSource = Array.Empty<LogEntry>();
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Initialize();
        }

        private void Initialize()
        {
            pipelineName = focusedPipeline?.name ?? string.Empty;
            titleContent = new UnityEngine.GUIContent($"Log: {pipelineName}");
            if (logEntryListView == null)
            {
                logEntryListView = rootVisualElement.Q<ListView>("logentry-list-view");
                logEntryListView.bindItem = OnBind;
                logEntryListView.makeItem = OnMake;
#if UNITY_2020_1_OR_NEWER
                logEntryListView.onItemsChosen += LogEntryListView_onItemsChosen;
#else
                logEntryListView.onItemChosen += LogEntryListView_onItemsChosen;
#endif
            }

            if (clearLogButton == null)
            {
                clearLogButton = rootVisualElement.Q<Button>("clear-log-button");
#if UNITY_2020_1_OR_NEWER
                clearLogButton.clicked += OnClearLog;
#else
                clearLogButton.clickable.clicked += OnClearLog;
#endif
            }

            Refresh();

            rootVisualElement.Bind(new SerializedObject(this));

            if (focusedPipeline)
            {
                focusedPipeline.LogUpdated -= Pipeline_LogUpdated;
                focusedPipeline.LogUpdated += Pipeline_LogUpdated;
            }
        }

#if UNITY_2020_1_OR_NEWER
        private void LogEntryListView_onItemsChosen(IEnumerable<object> obj) => LogContextWindow.ShowContext(obj.OfType<LogEntry>().First());
#else
        private void LogEntryListView_onItemsChosen(object obj) => LogContextWindow.ShowContext((LogEntry)obj);
#endif

        protected virtual void ShowButton(Rect r)
        {
            GUIStyle lockButton = "IN LockButton";
            locked = GUI.Toggle(r, locked, new GUIContent(), lockButton);
        }

        void OnBind(VisualElement element, int entryIndex)
        {
            var entry = (LogEntry)logEntryListView.itemsSource[entryIndex];
            var timeStamp = element.Q<Label>("time-stamp");

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
        }
        string LevelClass(LogLevel value) => $"{value}".ToLower();

        string IconClass(LogLevel logLevel) => $"{LevelClass(logLevel)}-icon";

        VisualElement OnMake() => GetTemplateInstance("LogEntryView").Children().First();

        void Pipeline_LogUpdated(object sender, LogEntry e) => Refresh();

        private void OnClearLog() => focusedPipeline.ClearLog();

        void Refresh()
        {
            if (focusedPipeline?.RunLog != null)
                logEntryListView.itemsSource = (IList)focusedPipeline.RunLog;
        }
    }
}
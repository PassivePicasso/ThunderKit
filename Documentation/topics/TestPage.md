```cs
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
        private ScrollView contextScrollView;
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
                logEntryListView.onSelectionChange += LogEntryListView_onSelectionChanged;
#else
                logEntryListView.onSelectionChanged += LogEntryListView_onSelectionChanged;
#endif
            }
            if (contextScrollView == null)
            {
                contextScrollView = rootVisualElement.Q<ScrollView>("context-scroll-view");
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
                focusedPipeline.LogUpdated += Pipeline_LogUpdated;
        }

#if UNITY_2020_1_OR_NEWER
        private void LogEntryListView_onSelectionChanged(IEnumerable<object> obj)
#else
        private void LogEntryListView_onSelectionChanged(List<object> obj)
#endif
        {
            contextScrollView.Clear();
            var entry = obj.OfType<LogEntry>().First();
            foreach (var context in entry.context)
            {
                var child = new MarkdownElement { Data = context, MarkdownDataType = MarkdownDataType.Text };
                //child.AddToClassList("log-entry-context");
                child.RefreshContent();
                contextScrollView.Add(child);
            }
        }

        protected virtual void ShowButton(Rect r)
        {
            GUIStyle lockButton = "IN LockButton";
            locked = GUI.Toggle(r, locked, new GUIContent(), lockButton);
        }


        private void OnGUI()
        {
            var contentView = contextScrollView?.Q<VisualElement>("ContentView");
            if (contentView == null)
                contentView = contextScrollView?.Q<VisualElement>("unity-content-container");

            if (contentView == null) return;
            if (contextScrollView == null) return;

            var scrollWidth = contextScrollView.contentRect.width;
            contentView.style.maxWidth = scrollWidth - 20;
            contextScrollView.style.maxHeight = rootVisualElement.contentRect.height / 2;
        }

        void OnBind(VisualElement element, int entryIndex)
        {
            var entry = (LogEntry)logEntryListView.itemsSource[entryIndex];
            var timeStamp = element.Q<Label>("time-stamp");
            var dateStamp = element.Q<Label>("date-stamp");

            var icon = element.Q<VisualElement>("icon-log-level");
            var messageElement = element.Q<MarkdownElement>("message-label");

            icon.EnableInClassList($"{LogLevel.Information}", false);
            icon.EnableInClassList($"{LogLevel.Warning}", false);
            icon.EnableInClassList($"{LogLevel.Error}", false);
            icon.EnableInClassList($"{entry.logLevel}", true);

            messageElement.Data = entry.message;
            messageElement.RefreshContent();
            timeStamp.text = entry.time.ToString(settings.TimeFormat);
            dateStamp.text = entry.time.ToString(settings.DateFormat);
            element.userData = entry.context;
        }

        VisualElement OnMake() => GetTemplateInstance("LogEntryView");

        void Pipeline_LogUpdated(object sender, LogEntry e) => Refresh();

        private void OnClearLog() => focusedPipeline.ClearLog();

        void Refresh()
        {
            if (focusedPipeline?.RunLog != null)
                logEntryListView.itemsSource = (IList)focusedPipeline.RunLog;
        }
    }
}
```
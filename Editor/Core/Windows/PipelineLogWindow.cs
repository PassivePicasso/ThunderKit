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
        private PipelineLogSettings settings;
        public string pipelineName;
        private ListView logEntryListView;
        private bool locked = false;
        private PipelineLog pipelineLog;


        public static void ShowLog(PipelineLog pipelineLog)
        {
            var consoleType = typeof(EditorWindow).Assembly.GetTypes().First(t => "ConsoleWindow".Equals(t.Name));
            var window = GetWindow<PipelineLogWindow>($"{pipelineLog.pipeline.name}", consoleType);
            window.pipelineLog = pipelineLog;
            window.settings = ThunderKitSetting.GetOrCreateSettings<PipelineLogSettings>();
            window.Initialize();
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
                this.pipelineLog = pipelineLog;
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
            if (!pipelineLog) return;
            pipelineName = pipelineLog.pipeline?.name ?? string.Empty;
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

            logEntryListView.itemsSource = (IList)pipelineLog.Entries;

            rootVisualElement.Bind(new SerializedObject(this));
        }

#if UNITY_2020_1_OR_NEWER
        private void UpdateContextWindow(IEnumerable<object> obj) => LogContextWindow.ShowContext(obj.OfType<LogEntry>().First());
#else
        private void UpdateContextWindow(object obj) => LogContextWindow.ShowContext((LogEntry)obj);
#endif
        private void UpdateContextWindowSelect(
#if UNITY_2020_1_OR_NEWER
            IEnumerable<object> obj
#else
            List<object> obj
#endif
            )
        {
            if (LogContextWindow.IsOpen)
                LogContextWindow.ShowContext(obj.OfType<LogEntry>().First());
        }

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
    }
}
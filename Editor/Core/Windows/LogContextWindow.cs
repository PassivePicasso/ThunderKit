using ThunderKit.Core.Pipelines;
using UnityEditor;
using ThunderKit.Markdown;
using System.Linq;
using ThunderKit.Core.UIElements;
using System.Collections.Generic;
using System;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Windows
{
    public class LogContextWindow : TemplatedWindow
    {
        internal static LogContextWindow instance;

        public LogEntry logEntry;
        private VisualElement popupSection;
        private VisualElement contentSection;
        public static bool IsOpen { get; private set; }
        public static LogContextWindow ShowContext(LogEntry logEntry)
        {
            if (!IsOpen || instance == null)
            {
                var content = EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow");
                content.text = "Log Inspector";
                instance = GetWindow<LogContextWindow>($"Log Inspector");
                instance.titleContent = content;
            }
            instance.logEntry = logEntry;
            instance.Initialize();

            return instance;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (!instance) instance = this;
            IsOpen = true;
            Initialize();
        }

        private void OnDestroy() => OnDisable();
        private void OnDisable()
        {
            IsOpen = false;
            instance = null;
        }

        private void Initialize()
        {
            if (popupSection == null) popupSection = rootVisualElement.Q<VisualElement>("popup-section");
            if (contentSection == null) contentSection = rootVisualElement.Q<VisualElement>("content-section");
            if (logEntry.context == null || logEntry.context.Length == 0) return;

            contentSection.Clear();
            var dataGroups = new List<String>();
            var elements = logEntry.context
                .Select(entry =>
                {
                    var header = entry;
                    var newLineIndex = entry.IndexOf("\r");
                    if (newLineIndex < 0)
                        newLineIndex = entry.IndexOf("\n");
                    if (newLineIndex >= 0)
                        header = entry.Substring(0, newLineIndex);
                    return header;
                }).ToArray();

            if (elements != null && elements.Any())
                dataGroups.AddRange(elements);
            PopupField<string> sectionSelector = null;

#if UNITY_2019_1_OR_NEWER
            if (dataGroups.Count == 0)
                sectionSelector = new PopupField<string>("Context");
            else
                sectionSelector = new PopupField<string>("Context", dataGroups, 0);
#elif UNITY_2018_1_OR_NEWER
            sectionSelector = new PopupField<string>(dataGroups, dataGroups.First());
#endif

            popupSection.Clear();
            popupSection.Add(sectionSelector);

            var markdownContent = new MarkdownElement { MarkdownDataType = MarkdownDataType.Text };
            void UpdateContext(string entry)
            {
                var data = entry;
                var newLineIndex = entry.IndexOf("\r");
                if (newLineIndex < 0)
                    newLineIndex = entry.IndexOf("\n");
                if (newLineIndex >= 0)
                    data = entry.Substring(newLineIndex);

                markdownContent.Data = data;
                markdownContent.RefreshContent();
            }
            if (logEntry.context != null && logEntry.context.Length > 0)
            {
                UpdateContext(logEntry.context[0]);
            }
            var stacktraceScrollView = new ScrollView();
            stacktraceScrollView.Add(markdownContent);
            stacktraceScrollView.StretchToParentSize();

#if UNITY_2019_1_OR_NEWER
            sectionSelector.RegisterValueChangedCallback(evt =>
#elif UNITY_2018_1_OR_NEWER
            sectionSelector.OnValueChanged(evt =>
#endif
            {
                var index = dataGroups.IndexOf(evt.newValue);
                UpdateContext(logEntry.context[index]);
            });

            contentSection.Add(stacktraceScrollView);
            rootVisualElement.Bind(new SerializedObject(this));
        }

        internal void Clear()
        {
            logEntry = default;
        }
    }
}
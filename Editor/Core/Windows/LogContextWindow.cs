using ThunderKit.Core.Pipelines;
using UnityEditor;
using ThunderKit.Markdown;
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
        public LogEntry logEntry;
        private ScrollView contextScrollView, stacktraceScrollView;
        private Button contextButton, stacktraceButton;
        private VisualElement panelSection;

        public static bool IsOpen { get; private set; }
        public static LogContextWindow ShowContext(LogEntry logEntry)
        {
            var content = EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow");
            content.text = "Log Inspector";
            var window = GetWindow<LogContextWindow>($"Log Inspector");
            window.titleContent = content;
            window.logEntry = logEntry;
            window.Initialize();
            return window;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Initialize();
            IsOpen = true;
        }
        private void OnDisable() => IsOpen = false;
        private void OnDestroy() => IsOpen = false;

        private void Initialize()
        {
            if (panelSection == null)
            {
                panelSection = rootVisualElement.Q<VisualElement>("panel-section");
            }
            if (contextScrollView == null)
            {
                contextScrollView = rootVisualElement.Q<ScrollView>("context-scroll-view");
                contextScrollView.StretchToParentSize();
#if UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
                contextScrollView.stretchContentWidth = true;
#endif
            }
            if (stacktraceScrollView == null)
            {
                stacktraceScrollView = rootVisualElement.Q<ScrollView>("stacktrace-scroll-view");
                stacktraceScrollView.StretchToParentSize();
#if UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
                stacktraceScrollView.stretchContentWidth = true;
#endif
            }

            if (contextButton == null)
            {
                contextButton = rootVisualElement.Q<Button>("context-button");
                contextButton.clickable.clicked += ContextClicked;
            }
            if (stacktraceButton == null)
            {
                stacktraceButton = rootVisualElement.Q<Button>("stacktrace-button");
                stacktraceButton.clickable.clicked += StacktraceClicked;
            }
            contextScrollView.Clear();

            stacktraceButton.visible = logEntry.exception != null;
            if (stacktraceButton.visible)
            {
                var child = new MarkdownElement { Data = logEntry.exception, MarkdownDataType = MarkdownDataType.Text };
                child.RefreshContent();
                stacktraceScrollView.Add(child);
                stacktraceScrollView.visible = false;
            }
            contextButton.visible = logEntry.context != null;
            if (contextButton.visible)
            {
                foreach (var context in logEntry.context)
                {
                    var child = new MarkdownElement { Data = context, MarkdownDataType = MarkdownDataType.Text };
                    //child.AddToClassList("log-entry-context");
                    child.RefreshContent();
                    contextScrollView.Add(child);
                }
                contextScrollView.visible = true;
            }
            rootVisualElement.Bind(new SerializedObject(this));
        }

        private void StacktraceClicked()
        {
            stacktraceScrollView.visible = true;
            contextScrollView.visible = false;
            UpdateClassState();
        }

        private void ContextClicked()
        {
            stacktraceScrollView.visible = false;
            contextScrollView.visible = true;
            UpdateClassState();
        }

        private void UpdateClassState()
        {
            stacktraceButton.EnableInClassList("active", stacktraceScrollView.visible);
            contextButton.EnableInClassList("active", contextScrollView.visible);
        }
    }
}
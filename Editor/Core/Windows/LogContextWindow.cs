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
    public class LogContextWindow : TemplatedWindow
    {
        public LogEntry logEntry;
        private ScrollView contextScrollView;


        public static LogContextWindow ShowContext(LogEntry logEntry)
        {
            //var consoleType = typeof(EditorWindow).Assembly.GetTypes().First(t => "ConsoleWindow".Equals(t.Name));
            var window = GetWindow<LogContextWindow>($"Log Inspector");
            window.logEntry = logEntry;
            window.Initialize();
            return window;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Initialize();
        }

        private void Initialize()
        {
            if (contextScrollView == null)
            {
                contextScrollView = rootVisualElement.Q<ScrollView>("context-scroll-view");
            }
            contextScrollView.Clear();

            if (logEntry.context != null)
                foreach (var context in logEntry.context)
                {
                    var child = new MarkdownElement { Data = context, MarkdownDataType = MarkdownDataType.Text };
                    //child.AddToClassList("log-entry-context");
                    child.RefreshContent();
                    contextScrollView.Add(child);
                }
            rootVisualElement.Bind(new SerializedObject(this));
        }
    }
}
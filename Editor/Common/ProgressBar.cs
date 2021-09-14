using System;
using UnityEditor;

namespace ThunderKit.Common.Logging
{
    public class ProgressBar : IDisposable
    {
        public ProgressBar(string title = null)
        {
            EditorUtility.DisplayProgressBar(title, $"", 0);
            Title = title;
        }

        public string Title { get; private set; }
        public string Message { get; private set; }

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }

        public void Update(string message = null, string title = null, float progress = 0)
        {
            Title = title ?? Title;
            Message = message ?? Message;
            EditorUtility.DisplayProgressBar(Title, Message, progress);
        }
    }
}
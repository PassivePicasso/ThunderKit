using System;
using UnityEditor;

namespace ThunderKit.Common.Logging
{
    public class ProgressBar : IDisposable
    {
        public ProgressBar(string title)
        {
            EditorUtility.DisplayProgressBar(title, $"", 0);
            Title = title;
        }

        public string Title { get; }

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }

        public void Update(string message, string title = null, float progress = 0)
        {
            title = title ?? Title;
            EditorUtility.DisplayProgressBar(Title, message, progress);
        }
    }
}
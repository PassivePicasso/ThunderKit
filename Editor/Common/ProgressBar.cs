using System;
using UnityEditor;
using UnityEngine;

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
        public float Progress { get; private set; }

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }

        public void Update(string message = null, string title = null, float progress = -1)
        {
            Title = title ?? Title;
            Message = message ?? Message;
            Progress = progress > -1 ? Mathf.Clamp(progress, 0, 1) : Progress;
            EditorUtility.DisplayProgressBar(Title, Message, Progress);
        }
    }
}
using UnityEditor;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.UIElements
{
    public class LoadingSpinner : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<LoadingSpinner> { }

        public bool started { get; private set; }

        private VisualElement innerElement;
        private int m_Rotation;

        public LoadingSpinner()
        {
            started = false;

            // add child elements to set up centered spinner rotation
            innerElement = new VisualElement();
            innerElement.AddToClassList("loading-spinner");
            innerElement.AddToClassList("hidden");
            Add(innerElement);
        }

        private void UpdateProgress()
        {
            transform.rotation = Quaternion.Euler(0, 0, m_Rotation);
            m_Rotation += 3;
            if (m_Rotation > 360)
                m_Rotation -= 360;
        }

        public void Start()
        {
            if (started)
                return;

            m_Rotation = 0;

            EditorApplication.update += UpdateProgress;

            started = true;
            innerElement.RemoveFromClassList("hidden");
        }

        public void Stop()
        {
            if (!started)
                return;

            EditorApplication.update -= UpdateProgress;

            started = false;
            innerElement.AddToClassList("hidden");
        }
    }
}

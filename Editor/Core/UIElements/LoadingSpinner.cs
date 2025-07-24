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
#if UNITY_2021_2_OR_NEWER
            // In Unity 2021.2 and later, transform origin of elements changed from
            // top-left to center by default.
            // https://discussions.unity.com/t/introducing-transform-styles/861646
            // To maintain the same behavior as before, we set the transform origin to top-left.
            style.transformOrigin = new TransformOrigin(Length.Percent(0), Length.Percent(0), 0);
#endif
            SetLoadingSpinnerBackground();

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

        private void SetLoadingSpinnerBackground()
        {
            // Unity 2019.1+ includes this as a loaded resource, but in earlier versions
            // we need to load it from the Unity Package Manager package's resources.
#if UNITY_2019_1_OR_NEWER
            innerElement.style.backgroundImage = new StyleBackground(
                (Texture2D)EditorGUIUtility.Load("icons/packagemanager/dark/loading.png")
            );
#else
            innerElement.style.backgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.unity.package-manager-ui/Editor/Resources/Images/Dark/loading.png");
#endif
        }
    }
}

using System;
using ThunderKit.Common;
using ThunderKit.Core.Windows;
using ThunderKit.Markdown;
using UnityEditor;
using System.IO;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.UIElements;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
#endif

namespace ThunderKit.Core.Windows
{
    public class MarkdownEditorWindow : TemplatedWindow
    {
        static MarkdownEditorWindow window;

        [OnOpenAsset]
#pragma warning disable IDE0060 // Parameter is part of Unity design
        public static bool DoubleClickDeploy(int instanceID, int line)
#pragma warning restore IDE0060 //  
        {
            if (!(EditorUtility.InstanceIDToObject(instanceID) is TextAsset textAsset)) return false;

            var assetPath = AssetDatabase.GetAssetPath(textAsset);
            var assetExtension = Path.GetExtension(assetPath);
            if (!assetExtension.Equals(".md", StringComparison.OrdinalIgnoreCase)) return false;

            if (window)
            {
                window.markdownFile = textAsset;
                window.OnEnable();
            }
            else
            {
                var instance = GetWindow<MarkdownEditorWindow>();
                instance.markdownFile = textAsset;
                instance.Show();
            }
            return true;
        }

        [MenuItem(Constants.ThunderKitMenuRoot + "Markdown Editor Window")]
        public static void ShowMarkdownEditorWindow() => GetWindow<MarkdownEditorWindow>();

        TextAsset markdownFile;
        VisualElement textArea, rendArea;
        VisualElement windowEditorArea;
        MarkdownElement renderer;
        TextField textfield;
        private Scroller scrollBar;

        public override void OnEnable()
        {
            base.OnEnable();

            MarkdownEditorWindow.window = this;

            textArea = rootVisualElement.Q("text-area");
            rendArea = rootVisualElement.Q("rend-area");
            scrollBar = rootVisualElement.Q<Scroller>("main-scroller");
            windowEditorArea = rootVisualElement.Q("markdown-editor-window-area");

            textfield = new TextField(int.MaxValue, true, false, '*');
            textfield.AddToClassList("text-editor");

            textfield.style.wordWrap = true;
            renderer = new MarkdownElement();
            renderer.MarkdownDataType = MarkdownDataType.Text;

#if UNITY_2019_1_OR_NEWER
            scrollBar.style.position = Position.Absolute;
            scrollBar.style.top =
            scrollBar.style.bottom = 0;
#else
            scrollBar.style.positionType = PositionType.Absolute;
            scrollBar.style.positionTop =
            scrollBar.style.positionBottom = 0;
#endif
            scrollBar.valueChanged -= OnScrollChanged;
            scrollBar.valueChanged += OnScrollChanged;

#if UNITY_2019_1_OR_NEWER
            textfield.style.position = Position.Absolute;
            textfield.style.top =
            textfield.style.left =
            textfield.style.right = 0;
#else
            textfield.style.positionType = PositionType.Absolute;
            textfield.style.positionTop =
            textfield.style.positionLeft =
            textfield.style.positionRight = 0;
#endif
            textArea.clippingOptions = VisualElement.ClippingOptions.ClipContents;
            rendArea.clippingOptions = VisualElement.ClippingOptions.ClipContents;

            textArea.Add(textfield);
            rendArea.Add(renderer);

            windowEditorArea.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
#if UNITY_2019_1_OR_NEWER
            textfield.RegisterValueChangedCallback(OnTextChanged); 
#else
            textfield.OnValueChanged(OnTextChanged);
#endif
            if (markdownFile)
            {
                textfield.value = markdownFile.text;
                rootVisualElement.RegisterCallback<WheelEvent>(OnWheel);
                rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
                textfield.RegisterCallback<KeyDownEvent>(OnKeyDown);
                textArea.RegisterCallback<KeyDownEvent>(OnKeyDown);
                rendArea.RegisterCallback<KeyDownEvent>(OnKeyDown);
                renderer.RegisterCallback<KeyDownEvent>(OnKeyDown);
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            var factor = (scrollBar.slider.pageSize * .05f) / Mathf.Max(renderer.ContentHeight, textfield.layout.height);
            scrollBar.value += evt.delta.y * factor;
        }

        private void OnScrollChanged(float obj)
        {
            var rTop = (renderer.layout.height - rendArea.layout.height) * obj;
            var tfTop = (textfield.layout.height - textArea.layout.height) * obj;
            renderer.style.positionTop = Mathf.Min(-rTop, 0);
            textfield.style.positionTop = Mathf.Min(-tfTop, 0);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.S:
                    if (!evt.ctrlKey) return;
                    var markdownPath = AssetDatabase.GetAssetPath(markdownFile);
                    File.WriteAllText(markdownPath, textfield.text);
                    AssetDatabase.ImportAsset(markdownPath, ImportAssetOptions.ForceUpdate);
                    break;
                case KeyCode.PageDown:
                    {
                        var factor = scrollBar.slider.pageSize / Mathf.Max(renderer.ContentHeight, textfield.layout.height);
                        scrollBar.value += factor;
                    }
                    break;
                case KeyCode.PageUp:
                    {
                        var factor = scrollBar.slider.pageSize / Mathf.Max(renderer.ContentHeight, textfield.layout.height);
                        scrollBar.value -= factor;
                    }
                    break;

            }

        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var rect = evt.newRect;
            float width = rect.width / 2;
            //var height = rect.height - (textfield.style.marginTop + textfield.style.marginBottom);
            //renderer.style.positionLeft = new StyleValue<float>(rect.width / 2);
            float halfBarWidth = (scrollBar.layout.width / 2);

            rendArea.style.width = width - halfBarWidth;
            textArea.style.width = width - halfBarWidth;

            scrollBar.style.positionLeft = width - halfBarWidth;
            scrollBar.slider.pageSize = scrollBar.slider.layout.height;

            var factor = scrollBar.slider.pageSize / Mathf.Max(renderer.ContentHeight, textfield.layout.height);
            scrollBar.slider.AdjustDragElement(factor);
        }

        public void OnDisable()
        {
            window = null;

#if UNITY_2019_1_OR_NEWER
            textfield.UnregisterValueChangedCallback(OnTextChanged);
#else
            textfield.RemoveOnValueChanged(OnTextChanged);
#endif
        }

        private void OnTextChanged(ChangeEvent<string> evt)
        {
            renderer.Data = evt.newValue;
            renderer.RefreshContent();

        }
    }
}
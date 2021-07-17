using System;
using ThunderKit.Common;
using ThunderKit.Core.Editor.Windows;
using ThunderKit.Markdown;
using UnityEditor;
using System.IO;
using UnityEditor.Callbacks;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
#endif

namespace ThunderKit.Core.Windows
{
    public class MarkdownEditorWindow : TemplatedWindow
    {
        [OnOpenAsset]
#pragma warning disable IDE0060 // Parameter is part of Unity design
        public static bool DoubleClickDeploy(int instanceID, int line)
#pragma warning restore IDE0060 //  
        {
            if (!(EditorUtility.InstanceIDToObject(instanceID) is TextAsset textAsset)) return false;

            var assetPath = AssetDatabase.GetAssetPath(textAsset);
            var assetExtension = Path.GetExtension(assetPath);
            if (!assetExtension.Equals(".md", StringComparison.OrdinalIgnoreCase)) return false;

            var window = GetWindow<MarkdownEditorWindow>();
            window.markdownFile = textAsset;
            window.RefreshContent();
            window.Show();

            return true;
        }

        [MenuItem(Constants.ThunderKitMenuRoot + "Markdown Editor Window")]
        public static void ShowMarkdownEditorWindow() => GetWindow<MarkdownEditorWindow>();

        private TextAsset markdownFile;

        private VisualElement editorArea;

        private MarkdownElement renderer;
        private ScrollView rendererScroll;
        private ScrollView editorScroll;
        private TextField textfield;
        private Label loadedFileLabel;

        Scroller EditorScroller => editorScroll.verticalScroller;
        Scroller RendererScroller => rendererScroll.verticalScroller;
        public override void OnEnable()
        {
            base.OnEnable();
            RefreshContent();
        }

        private void Initialize()
        {
            editorArea = rootVisualElement.Q("editor-area");
            textfield = rootVisualElement.Q("markdown-textfield") as TextField;// new TextField(int.MaxValue, true, false, '*');
            renderer = rootVisualElement.Q("markdown-renderer") as MarkdownElement;
            rendererScroll = rootVisualElement.Q("renderer-scroll") as ScrollView;
            editorScroll = rootVisualElement.Q("editor-scroll") as ScrollView;
            loadedFileLabel = rootVisualElement.Q("loaded-file-label") as Label;

            editorScroll.verticalScroller.valueChanged -= OnEditorScrollChanged;
            editorScroll.verticalScroller.valueChanged += OnEditorScrollChanged;
            rendererScroll.verticalScroller.valueChanged -= OnRendererScrollChanged;
            rendererScroll.verticalScroller.valueChanged += OnRendererScrollChanged;
            textfield.labelElement.RemoveFromHierarchy();
            textfield.tripleClickSelectsLine = true;
            textfield.doubleClickSelectsWord = true;
            //Re-assert callbacks
            rootVisualElement.UnregisterCallback<KeyDownEvent>(OnSave);
            textfield.UnregisterCallback<KeyDownEvent>(OnSave);
            editorArea.UnregisterCallback<KeyDownEvent>(OnSave);
            renderer.UnregisterCallback<KeyDownEvent>(OnSave);

            rootVisualElement.RegisterCallback<KeyDownEvent>(OnSave);
            textfield.RegisterCallback<KeyDownEvent>(OnSave);
            editorArea.RegisterCallback<KeyDownEvent>(OnSave);
            renderer.RegisterCallback<KeyDownEvent>(OnSave);

#if UNITY_2019_1_OR_NEWER
            textfield.UnregisterValueChangedCallback(OnTextChanged);
            textfield.RegisterValueChangedCallback(OnTextChanged);
#else
            textfield.RemoveOnValueChanged(OnTextChanged);
            textfield.OnValueChanged(OnTextChanged);
#endif
        }

        void OnRendererScrollChanged(float value) => UpdateScrollers(RendererScroller, EditorScroller, value);
        void OnEditorScrollChanged(float value) => UpdateScrollers(EditorScroller, RendererScroller, value);
        void UpdateScrollers(Scroller master, Scroller slave, float value)
        {
            var newPercentage = value / master.highValue;
            var currentPercentage = slave.value / slave.highValue;
            if (Mathf.Abs(newPercentage - currentPercentage) > .0001f)
            {
                slave.value = slave.highValue * newPercentage;
            }
        }

        private void RefreshContent()
        {
            if(editorArea == null)
                Initialize();

            if (markdownFile)
            {
                loadedFileLabel.text = markdownFile.name;
                textfield.value = markdownFile.text;
                renderer.Data = markdownFile.text;
                renderer.RefreshContent();
            }
            UpdateTextFieldHeight();
        }

        private void UpdateTextFieldHeight()
        {
            var textSize = textfield.MeasureTextSize(textfield.text,
                textfield.contentRect.width,
                VisualElement.MeasureMode.AtMost,
                float.PositiveInfinity,
                VisualElement.MeasureMode.Undefined
                );
            textfield.style.height = textSize.y;
        }

        private void OnSave(KeyDownEvent evt)
        {
            if (!evt.ctrlKey) return;
            if (evt.keyCode != KeyCode.S) return;

            var markdownPath = AssetDatabase.GetAssetPath(markdownFile);
            File.WriteAllText(markdownPath, textfield.text);
            AssetDatabase.ImportAsset(markdownPath, ImportAssetOptions.ForceUpdate);
        }

        public void OnDisable()
        {
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

using System;
using ThunderKit.Common;
using ThunderKit.Core.Editor.Windows;
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
            window.Show();

            return true;
        }

        [MenuItem(Constants.ThunderKitMenuRoot + "Markdown Editor Window")]
        public static void ShowMarkdownEditorWindow() => GetWindow<MarkdownEditorWindow>();

        TextAsset markdownFile;
        VisualElement editorArea;
        MarkdownElement renderer;
        TextField textfield;
        public override void OnEnable()
        {
            base.OnEnable();

            editorArea = rootVisualElement.Q("editor-area");

            textfield = new TextField(int.MaxValue, true, false, '*');
#if UNITY_2019_1_OR_NEWER
            textfield.style.position = Position.Absolute;
            textfield.style.left =
            textfield.style.top =
            textfield.style.bottom = 0;
#else
            textfield.style.wordWrap = true;
            textfield.style.positionType = PositionType.Absolute;
            textfield.style.positionLeft =
            textfield.style.positionTop =
            textfield.style.positionBottom = 0f;
#endif
            renderer = new MarkdownElement();
#if UNITY_2019_1_OR_NEWER
            textfield.style.position = Position.Absolute;
            textfield.style.right =
            textfield.style.top =
            textfield.style.bottom = 0;
#else
            renderer.style.positionType = PositionType.Absolute;
            renderer.style.positionRight =
            renderer.style.positionTop =
            renderer.style.positionBottom = 0;
#endif

            editorArea.Add(textfield);
            editorArea.Add(renderer);

            editorArea.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
#if UNITY_2019_1_OR_NEWER
            textfield.RegisterValueChangedCallback(OnTextChanged); 
#else
            textfield.OnValueChanged(OnTextChanged);
#endif

            if (markdownFile)
            {
                textfield.SetValueAndNotify(markdownFile.text);
                rootVisualElement.RegisterCallback<KeyDownEvent>(OnSave);
                textfield.RegisterCallback<KeyDownEvent>(OnSave);
                editorArea.RegisterCallback<KeyDownEvent>(OnSave);
                renderer.RegisterCallback<KeyDownEvent>(OnSave);
            }
        }

        private void OnSave(KeyDownEvent evt)
        {
            if (!evt.ctrlKey) return;
            if (evt.keyCode != KeyCode.S) return;

            var markdownPath = AssetDatabase.GetAssetPath(markdownFile);
            File.WriteAllText(markdownPath, textfield.text);
            AssetDatabase.ImportAsset(markdownPath, ImportAssetOptions.ForceUpdate);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var rect = evt.newRect;
            float width = rect.width / 2;
            //var height = rect.height - (textfield.style.marginTop + textfield.style.marginBottom);
            //renderer.style.positionLeft = new StyleValue<float>(rect.width / 2);
#if UNITY_2019_1_OR_NEWER
            textfield.style.right = width;
            renderer.style.left = width;
#else
            textfield.style.positionRight = width;
            renderer.style.positionLeft = width;
#endif
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

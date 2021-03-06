using UnityEngine;
using System;
using UnityEditor;
using System.Linq;
using ThunderKit.Core.Editor.Controls;
using System.IO;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#elif UNITY_2018_1_OR_NEWER
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

using static ThunderKit.Core.Editor.ScriptEditorHelper;

namespace ThunderKit.Core.Editor.Windows
{
    public class AddComposableElementWindow : TemplatedWindow
    {
        //public static void ShowPopup() => 
        static AddComposableElementWindow _instance;
        public static bool HasAssetToAdd()
        {
            return NewScriptInfo.Instance.addAsset;

        }
        public static void Backup(Func<MonoScript, ScriptableObject> onCreateScript)
        {
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<AddComposableElementWindow>();
            }
            _instance.Create = onCreateScript;
            if (NewScriptInfo.Instance.addAsset)
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(NewScriptInfo.Instance.scriptPath);
                var instance = _instance.Create(script);
                if (instance)
                {
                    EditScript(instance);
                    NewScriptInfo.Instance.Reset();
                }
            }
        }

        public bool StaysOpen = false;
        public string ScriptTemplate;
        public string ScriptPath;

        private Func<MonoScript, bool> filter;
        public Func<MonoScript, bool> Filter
        {
            get => filter;
            set
            {
                filter = value;
                Refresh();
            }
        }

        public Func<MonoScript, ScriptableObject> Create { get; internal set; }

        private ListView scriptList;
        private Type baseType;
        private MonoScript[] monoScripts;
        private string SearchString;
        private TextField searchField;
        private ListView folderList;
        private TextField nameField;

        public void OnEnable()
        {
            Construct();
        }
        private void OnLostFocus()
        {
            if (StaysOpen) return;

            Close();
            DestroyImmediate(this);
        }

        public void Refresh()
        {
            if (scriptList != null)
            {

                monoScripts = Resources.FindObjectsOfTypeAll<MonoScript>()
                                           .Where(filter)
                                           .Where(ms => ms.name.ToLower().Contains((SearchString ?? string.Empty).ToLower()))
                                           .OrderBy(ms => ms.name)
                                           .ToArray();
                scriptList.itemsSource = monoScripts;
            }
        }

        private void Construct()
        {
            rootVisualContainer.Clear();

            GetTemplateInstance("AddComponentView", rootVisualContainer);

            scriptList = rootVisualContainer.Q<ListView>("script-list");
            scriptList.makeItem = MakeComponentView;
            scriptList.bindItem = BindComponentView;
            scriptList.onItemChosen += OnChoose;

            searchField = rootVisualContainer.Q<TextField>("searchField");
            searchField.RegisterCallback<ChangeEvent<string>>(OnSearchText);
            searchField.SetValueWithoutNotify(SearchString);
            searchField.RegisterCallback<AttachToPanelEvent>(OnAttach);

            var newScriptButton = rootVisualContainer.Q<Button>("new-script-button");

            newScriptButton.clickable.clickedWithEventInfo += OnNewScript;
        }
        private void OnChoose(object obj)
        {
            Create?.Invoke(obj as MonoScript);
            Close();
            DestroyImmediate(this);
        }

        private void OnAttach(AttachToPanelEvent evt) => (evt.currentTarget as Focusable)?.Focus();

        private void OnSearchText(ChangeEvent<string> evt)
        {
            SearchString = evt.newValue;
            Refresh();
        }

        VisualElement MakeComponentView()
        {
            var element = GetTemplateInstance("ComponentView");
            element.name = "ComponentView";
            return element;
        }

        private void BindComponentView(VisualElement element, int index) => element.Q<Label>("script-name").text = monoScripts[index].name;

        private void OnNewScript(EventBase obj)
        {
            rootVisualContainer.Clear();

            GetTemplateInstance("CreateComponentView", rootVisualContainer);

            var createScriptButton = rootVisualContainer.Q<Button>("create-script-button");
            createScriptButton.clickable.clickedWithEventInfo += OnCreateScript;
            var backButton = rootVisualContainer.Q<Button>("back-button");
            backButton.clickable.clickedWithEventInfo += OnBackButton;


            folderList = rootVisualContainer.Q<ListView>("folder-list");
            folderList.selectionType = SelectionType.None;
            folderList.makeItem = MakeFolderView;
            folderList.bindItem = BindFolderView;

            nameField = rootVisualContainer.Q<TextField>("nameField");
            nameField.RegisterCallback<ChangeEvent<string>>(OnNameComponent);
            nameField.RegisterCallback<AttachToPanelEvent>(OnAttach);
            nameField.value = SearchString = ScriptPath = string.Empty;
            RefreshFolders();
        }

        private void OnBackButton(EventBase obj)
        {
            Construct();
            Refresh();
        }

        private void OnNameComponent(ChangeEvent<string> evt)
        {
            ScriptPath = evt.newValue;
            RefreshFolders();
        }
        void BindFolderView(VisualElement folderView, int index)
        {
            string folder = $"{folderList.itemsSource[index]}";
            folderView.Q<Label>().text = folder;
        }

        private void RefreshFolders()
        {
            string targetPath = string.IsNullOrEmpty(ScriptPath) ? "Assets" : Path.Combine("Assets", ScriptPath);
            var folder = AssetDatabase.IsValidFolder(targetPath) ? targetPath : Path.GetDirectoryName(targetPath);
            folderList.itemsSource = Directory.EnumerateDirectories(folder, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).Where(f => !f.StartsWith(".")).ToArray();
        }

        VisualElement MakeFolderView()
        {
            var folderView = GetTemplateInstance("FolderView");
            var label = folderView.Q<Label>();
            folderView.AddManipulator(new Clickable(() =>
            {
                ScriptPath = $"{ScriptPath}{label.text}/";
                nameField.value = ScriptPath;
                nameField.RegisterCallback<FocusEvent>(OnNameFieldFocus);
                void OnNameFieldFocus(FocusEvent evt)
                {
                    nameField.UnregisterCallback<FocusEvent>(OnNameFieldFocus);
                    nameField.SelectRange(nameField.value.Length, 0);
                }
                nameField.Focus();
            }));
            return folderView;
        }


        private void OnCreateScript(EventBase obj)
        {
            GenerateAndLoadScript(ScriptTemplate, ScriptPath);
            Close();
            DestroyImmediate(this);
        }

    }
}
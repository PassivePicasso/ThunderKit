using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Editor.Controls
{
    public class AddScriptWindow : EditorWindow
    {
        private class Styles
        {
            public static GUIStyle header = "DD HeaderStyle";
            public static GUIStyle itemStyle = "DD ItemStyle";
            public static GUIStyle background = "grey_border";
            public static GUIStyle rightArrow = "ArrowNavigationRight";
            public static GUIStyle leftArrow = "ArrowNavigationLeft";
            public static GUIStyle addButtonStyle = new GUIStyle(GUI.skin.button);
            public static Texture2D blueTexture;

            static Styles()
            {
                blueTexture = EditorGUIUtility.whiteTexture;
                blueTexture.SetPixels(EditorGUIUtility.whiteTexture.GetPixels32(0).Select(p => new Color(62f / 255f, 125f / 255f, 231f / 255f, 1f)).ToArray());
                blueTexture.Apply();
                addButtonStyle.fontSize = 12;
                addButtonStyle.fixedWidth = 230;
                addButtonStyle.fixedHeight = 23;
            }
        }

        static AddScriptWindow _instance;

        Action<MonoScript> CreateScriptDelegate;
        Func<MonoScript, bool> FilerScriptDelegate;

        Type elementType;
        Vector2 _scrollPosition;
        private Vector2 s_IconSize = new Vector2(13, 13);
        bool _activeParent = true;
        string _typeFullName = "NewEquipmentBehaviourScript";
        string _searchString = string.Empty;
        string _template;

        public static bool HasAssetToAdd()
        {
            return AddScriptWindowBackup.Instance.addAsset;
        }

        public static void Show(Rect position, Type elementType, Action<MonoScript> onCreateScript, Func<MonoScript, bool> onFilerScript, string template)
        {
            if (_instance == null)
                _instance = CreateInstance<AddScriptWindow>();

            var hasAssetToAdd = HasAssetToAdd();
            EditorGUI.BeginDisabledGroup(hasAssetToAdd);
            if (GUI.Button(position, $"Add {ObjectNames.NicifyVariableName(elementType.Name)}", Styles.addButtonStyle))
            {
                _instance.elementType = elementType;
                _instance.Init(position, onCreateScript, onFilerScript, template);
                _instance.Repaint();
            }
            EditorGUI.EndDisabledGroup();

            if (hasAssetToAdd)
                Backup(onCreateScript);
        }

        public static void Backup(Action<MonoScript> onCreateScript)
        {
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<AddScriptWindow>();
            }
            _instance.CreateScriptDelegate = onCreateScript;
            if (AddScriptWindowBackup.Instance.addAsset)
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AddScriptWindowBackup.Instance.scriptPath);
                if (script?.GetClass() == null)
                {
                    AddScriptWindowBackup.Instance.addAsset = false;
                    return;
                }
                _instance.CreateScriptDelegate(script);
                AddScriptWindowBackup.Instance.Reset();
            }
        }

        private void Init(Rect rect, Action<MonoScript> onCreateScript, Func<MonoScript, bool> onFilerScript, string template)
        {
            var v2 = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
            rect.x = v2.x - 1;
            rect.y = v2.y;

            //CreateComponentTree();
            ShowAsDropDown(rect, new Vector2(rect.width, 320f));
            Focus();
            wantsMouseMove = true;
            CreateScriptDelegate = onCreateScript;
            FilerScriptDelegate = onFilerScript;
            _template = template;
        }

        void OnGUI()
        {
            GUI.Label(new Rect(0.0f, 0.0f, this.position.width, this.position.height), GUIContent.none, Styles.background);

            GUILayout.Space(7);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!_activeParent);
            _searchString = GUILayout.TextField(_searchString, GUI.skin.FindStyle("SearchTextField"));
            var buttonStyle = _searchString == string.Empty ? GUI.skin.FindStyle("SearchCancelButtonEmpty") : GUI.skin.FindStyle("SearchCancelButton");
            if (GUILayout.Button(string.Empty, buttonStyle))
            {
                _searchString = string.Empty;
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            if (_activeParent)
            {
                _typeFullName = _searchString;
                ListGUI();
            }
            else
            {
                NewScriptGUI();
            }
        }

        void ListGUI()
        {
            var rect = position;
            rect.x = +1f;
            rect.y = 30f;
            rect.height -= 30f;
            rect.width -= 2f;
            bool isRepaint = Event.current.type == EventType.Repaint;
            GUILayout.BeginArea(rect);

            rect = GUILayoutUtility.GetRect(10f, 25f);
            GUI.Label(rect, _searchString == string.Empty ? ObjectNames.NicifyVariableName(elementType.Name) : "Search", Styles.header);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            var scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            var searchString = _searchString.ToLower();
            foreach (var script in scripts)
            {
                var origColor = GUI.color;
                if (!script || script.GetClass() == null || !FilerScriptDelegate(script))
                    continue;

                if (searchString != string.Empty && !script.name.ToLower().Contains(searchString))
                    continue;

                var content = new GUIContent(ObjectNames.NicifyVariableName(script.name), AssetPreview.GetMiniTypeThumbnail(script.GetClass()));
                var buttonRect = GUILayoutUtility.GetRect(content, Styles.itemStyle, GUILayout.ExpandWidth(true));
                buttonRect = new Rect(buttonRect.x, buttonRect.y, buttonRect.width, buttonRect.height);
                EditorGUIUtility.SetIconSize(s_IconSize);

                bool selected = buttonRect.Contains(Event.current.mousePosition);
                if (selected) Repaint();

                if (isRepaint)
                    Styles.itemStyle.Draw(buttonRect, content, false, false, selected, selected);

                if (Event.current.type == EventType.MouseUp && selected)
                {
                    CreateScriptDelegate(script);
                    //CreateScriptInstance(script);
                    Close();
                }
                EditorGUIUtility.SetIconSize(Vector2.zero);

                GUI.color = origColor;
            }

            var newContent = new GUIContent($"    New {ObjectNames.NicifyVariableName(elementType.Name)} Script");
            var rect2 = GUILayoutUtility.GetRect(newContent, Styles.itemStyle, GUILayout.ExpandWidth(true));
            rect2 = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);

            bool newSelected = rect2.Contains(Event.current.mousePosition);
            if (newSelected) Repaint();
            newContent.image = null;
            if (isRepaint)
                Styles.itemStyle.Draw(rect2, newContent, false, false, newSelected, newSelected);

            if (Event.current.type == EventType.MouseUp && newSelected) _activeParent = false;

            GUI.Label(new Rect((float)(rect2.x + (double)rect2.width - 22.0), rect2.y, 13f, 13f), "", Styles.rightArrow);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void NewScriptGUI()
        {
            var rect = position;
            rect.x = +1f;
            rect.y = 30f;
            rect.height -= 30f;
            rect.width -= 2f;
            GUILayout.BeginArea(rect);

            rect = GUILayoutUtility.GetRect(10f, 25f);
            GUI.Label(rect, "New Script", Styles.header);

            GUILayout.Label("Name", EditorStyles.label);
            EditorGUI.FocusTextInControl("NewScriptName");
            GUI.SetNextControlName("NewScriptName");
            _typeFullName = EditorGUILayout.TextField(_typeFullName);

            EditorGUILayout.Space();
            string error;
            bool flag = CanCreate(out error);
            if (!flag && _typeFullName != string.Empty)
            {
                GUILayout.Label(error, EditorStyles.helpBox);
            }


            EditorGUI.BeginDisabledGroup(!flag);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create and Add"))
            {
                GenerateAndLoadScript();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndArea();
        }

        private bool CanCreate(out string error)
        {
            error = string.Empty;
            if (_typeFullName == string.Empty) return false;
            if (ClassAlreadyExists())
                error = "A class called \"" + _typeFullName + "\" already exists.";
            else if (ClassNameIsInvalid())
                error = "The script name may only consist of a-z, A-Z, 0-9, _.";
            else
                return true;
            return false;
        }

        private bool ClassNameIsInvalid() => !CodeGenerator.IsValidLanguageIndependentIdentifier(GetDetails().fileName);

        private (string destinationPath, string nameSpace, string fileName) GetDetails() => (
            destinationPath: Path.Combine("Assets", $"{_typeFullName}.cs").Replace("\\", "/"),
            nameSpace: Path.GetDirectoryName(_typeFullName).Replace('/', '.').Replace('\\', '.'),
            fileName: Path.GetFileNameWithoutExtension(_typeFullName)
        );

        private bool ClassAlreadyExists() => _typeFullName == string.Empty ? false : AssetDatabase.FindAssets($"{GetDetails().fileName}.cs").Any();

        void GenerateAndLoadScript()
        {
            var (destinationPath, ns, fileName) = GetDetails();

            if (string.IsNullOrEmpty(_template)) return;

            var backup = AddScriptWindowBackup.Instance;
            backup.addAsset = true;
            backup.scriptPath = destinationPath;
            EditorUtility.SetDirty(backup);

            var parentDirectoryPath = Path.GetDirectoryName(destinationPath);
            Directory.CreateDirectory(parentDirectoryPath);

            ns = string.IsNullOrEmpty(ns) ? "Assets" : ns;
            var rendered = string.Format(_template, ns, fileName);
            File.WriteAllText(destinationPath, rendered);

            AssetDatabase.ImportAsset(destinationPath);
            AssetDatabase.Refresh();
            Close();

        }
    }
}
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace ThunderKit.Core.Editor
{
    public class SearchSuggest<T>
    {
        public Func<string, IEnumerable<T>> Evaluate;
        public Func<int, T, bool> OnSuggestionGUI;
        public float itemHeight = singleLineHeight;

        PickerWindow popupWindow;
        SearchField searchField;
        string searchString = string.Empty;

        IEnumerable<T> searchEnumeration = Enumerable.Empty<T>();


        private void OnEnable()
        {
            Selection.selectionChanged += SelectionChanged;
        }

        private void SelectionChanged() => Cleanup();

        private void OnDestroy() => Cleanup();

        /// <summary>
        /// Render SearchSuggest
        /// </summary>
        /// <returns>true if EditorWindow should Repaint, otherwise false</returns>
        public bool OnSuggestGUI(ref Rect rect, string label)
        {
            if (searchField == null)
                searchField = new SearchField { autoSetFocusOnFindCommand = true };

            var labelRect = new Rect(rect.position,
                            new Vector2(labelWidth, singleLineHeight + 4));

            var offset = Vector2.right * labelWidth;
            var fieldRect = new Rect(rect.position + offset, rect.size - offset);

            GUI.Label(labelRect, label);

            EditorGUI.BeginChangeCheck();
            searchString = searchField.OnGUI(fieldRect, searchString);
            if (EditorGUI.EndChangeCheck())
            {
                searchEnumeration = Evaluate(searchString);
                if (popupWindow) popupWindow.Repaint();
                return true;
            }
            ShowOptions(fieldRect, searchEnumeration);

            rect = new Rect(rect.x, rect.y + (singleLineHeight + standardVerticalSpacing), rect.width, rect.height);

            return false;
        }

        private void ShowOptions(Rect rect, IEnumerable<T> options)
        {
            if (!searchField.HasFocus() || searchString.Length == 0 || !options.Any()) Cleanup();
            else if (popupWindow)
            {
                if (options.Any()) popupWindow.options = options.OfType<object>().ToList();

                popupWindow.itemHeight = itemHeight;
                popupWindow.OnItemGUI = (index, obj) => OnSuggestionGUI(index, (T)obj);
                if (Event.current.type == EventType.Repaint)
                    popupWindow.UpdatePopupPosition(rect);
            }
            else if (searchField.HasFocus() && options.Any())
            {
                popupWindow = ScriptableObject.CreateInstance<PickerWindow>();
                popupWindow.itemHeight = itemHeight;
                popupWindow.options = options.OfType<object>().ToList();
                popupWindow.OnItemGUI = (index, obj) => OnSuggestionGUI(index, (T)obj);
                popupWindow.Show(rect);
            }
        }

        public void Cleanup()
        {
            searchString = string.Empty;
            if (popupWindow)
            {
                popupWindow.Close();
                UnityEngine.Object.DestroyImmediate(popupWindow);
            }
        }

    }
}
#endif
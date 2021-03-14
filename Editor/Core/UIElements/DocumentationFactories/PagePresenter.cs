using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using ThunderKit.Core.Editor;
using System.Linq;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#else
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace ThunderKit.Core.UIElements
{
    using static TemplateHelpers;
    public class ChosenListViewPresenterFactory : VisualElement.UxmlFactory
    {
        public override string uxmlName => "PagePresenter";

        public override string uxmlQualifiedName => uxmlName;

        public override string substituteForTypeName => uxmlName;

        public override string substituteForTypeNamespace => uxmlName;

        public override string substituteForTypeQualifiedName => uxmlName;

        public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
        {
            var listviewnameFound = bag.TryGetAttributeValue("list-view-name", out var listviewname);
            if (listviewnameFound)
            {
                var templateContainer = base.Create(bag, cc);
                templateContainer.AddToClassList("page-presenter");
                templateContainer.userData = listviewname;
                var visualTreeAsset = cc.visualTreeAsset;
                templateContainer.RegisterCallback<AttachToPanelEvent>(OnAttached);

                return templateContainer;

                void OnAttached(AttachToPanelEvent evt)
                {
                    var target = evt.target as VisualElement;
                    var lvn = target.userData as string;
                    var root = target.parent;
                    while (root != null && !(root is TemplateContainer))
                        root = root.parent;

                    var listView = root.Query().Descendents<ListView>(lvn).Build().First();
                    if (listView != null)
                    {
                        target.UnregisterCallback<AttachToPanelEvent>(OnAttached);
                        listView.onSelectionChanged += ListView_onSelectionChanged;

                        if (listView.itemsSource.Count > 0 && listView.selectedIndex >= 0)
                        {
                            target.Clear();
                            LoadTemplateRelative(GetAssetDirectory(visualTreeAsset), listView.itemsSource[listView.selectedIndex] as string, target);
                        }
                        void ListView_onSelectionChanged(List<object> obj)
                        {
                            var templatePath = obj.OfType<string>().First();
                            target.Clear();
                            LoadTemplateRelative(GetAssetDirectory(visualTreeAsset), templatePath, target);
                        }
                    }
                }
            }

            throw new Exception($"PagePresenter missing list-view-name attribute\r\n{AssetDatabase.GetAssetPath(cc.visualTreeAsset)}");
        }
    }
}
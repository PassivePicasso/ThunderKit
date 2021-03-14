using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
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
    public class PageListViewFactory : ListView.UxmlFactory
    {
        private const string LabelClass = "pagelistview__label";
        private const string ElementClass = "pagelistview__item";
        private const string FallBackClass = ElementClass + "--fallback";

        public override string uxmlName => "PageListView";

        public override string uxmlQualifiedName => uxmlName;

        public override string substituteForTypeName => uxmlName;

        public override string substituteForTypeNamespace => uxmlName;

        public override string substituteForTypeQualifiedName => uxmlName;

        public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
        {
            var listView = base.Create(bag, cc) as ListView;

            var treeAssetPath = AssetDatabase.GetAssetPath(cc.visualTreeAsset);
            void BindListViewItem(VisualElement ve, int index)
            {
                var item = listView.itemsSource[index] as string;
                var label = ve.Q<Label>(className: LabelClass);
                label.text = Path.GetFileNameWithoutExtension(item);
            }
            listView.bindItem = BindListViewItem;
            string nameValue;
            if (bag.TryGetAttributeValue(nameof(VisualElement.name), out nameValue))
                listView.name = nameValue;

            string pagepath, itemtemplate, searchoption;
            var itemtemplateFound = bag.TryGetAttributeValue("item-template", out itemtemplate);
            var searchoptionFound = bag.TryGetAttributeValue("search-option", out searchoption);
            if (bag.TryGetAttributeValue(nameof(pagepath), out pagepath))
            {
                string projectPath = pagepath;
                if (!projectPath.StartsWith("Assets") && !projectPath.StartsWith("Packages"))
                {
                    var treeAssetDirectory = Path.GetDirectoryName(treeAssetPath);
                    projectPath = Path.Combine(treeAssetDirectory, pagepath);
                }
                var pageFiles = Directory.EnumerateFiles(projectPath, "*.uxml", SearchOption.TopDirectoryOnly).ToArray();
                var directories = Directory.EnumerateDirectories(projectPath, "*", SearchOption.TopDirectoryOnly).ToArray();

                EditorApplication.projectChanged += () => listView.itemsSource = Directory.EnumerateFiles(projectPath, "*.uxml", SearchOption.TopDirectoryOnly).ToArray();

                if (itemtemplateFound)
                {
                    listView.makeItem = MakeListViewItem;
                    VisualElement MakeListViewItem()
                    {
                        var rootpath = GetAssetDirectory(cc.visualTreeAsset);
                        var element = LoadTemplateRelative(rootpath, itemtemplate);
                        element.AddToClassList(ElementClass);
                        return element;
                    }
                }
                else
                {
                    listView.makeItem = MakeListViewItem;
                    VisualElement MakeListViewItem()
                    {
                        var element = new Label();
                        element.AddToClassList(FallBackClass);
                        element.AddToClassList(LabelClass);
                        element.AddToClassList(ElementClass);
                        return element;
                    }
                }

                listView.itemsSource = pageFiles;
                listView.selectedIndex = 0;
            }
            return listView;
        }
    }
}
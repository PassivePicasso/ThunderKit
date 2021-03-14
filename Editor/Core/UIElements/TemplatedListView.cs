using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
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
    public class TemplatedListViewFactory : ListView.UxmlFactory
    {
        public override string uxmlName => "TemplatedListView";

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
                var item = listView.itemsSource[index] as UnityEngine.Object;
                if (!item) throw new ArgumentException($"TemplatedListView Can only bind to UnityEngine.Object types\r\n{treeAssetPath}");

                BindingExtensions.Bind(ve, new SerializedObject(item));
            }
            listView.bindItem = BindListViewItem;
            var templateAttributeFound = bag.TryGetAttributeValue("template", out var template);
            if (templateAttributeFound)
            {
                listView.makeItem = MakeListViewItem;
                VisualElement MakeListViewItem() => LoadTemplateRelative(GetAssetDirectory(cc.visualTreeAsset), template);
            }
            return listView;
        }
    }
}
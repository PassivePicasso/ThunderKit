using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using ThunderKit.Core.Editor;
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
    public class TemplatePresenterFactory : VisualElement.UxmlFactory
    {
        public override string uxmlName => "TemplatePresenter";

        public override string uxmlQualifiedName => uxmlName;

        public override string substituteForTypeName => uxmlName;

        public override string substituteForTypeNamespace => uxmlName;

        public override string substituteForTypeQualifiedName => uxmlName;

        public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
        {
            var templateAttributeFound = bag.TryGetAttributeValue("template", out var template);
            if (templateAttributeFound)
            {
                var templateInstance = LoadTemplateRelative(cc.visualTreeAsset, template);
                return templateInstance;
            }

            throw new Exception($"TemplatePresenter missing template attribute\r\n{AssetDatabase.GetAssetPath(cc.visualTreeAsset)}");
        }
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;
using ThunderKit.Core.Editor;
using System;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.Core.UIElements;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Integrations.Thunderstore
{
    // Create a new type of Settings Asset.
    public class ThunderstoreSettings : ThunderKitSetting
    {
        const string TemplatePath = "Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/ThunderstoreTemplate.uxml";

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            var thunderstoreSettingsTemplate = TemplateHelpers.LoadTemplateInstance(TemplatePath);

            var sourceList = thunderstoreSettingsTemplate.Q<ListView>("thunderstore-sources-list");

            var sources = AssetDatabase.FindAssets("t:PackageSource", new[] { "Assets", "Packages" })
                      .Select(AssetDatabase.GUIDToAssetPath)
                      .Select(AssetDatabase.LoadAssetAtPath<PackageSource>)
                      .OfType<ThunderstoreSource>()
                      .ToArray();
            sourceList.selectionType = SelectionType.None;
            sourceList.makeItem = () =>
            {
                var subcontainer = new VisualElement();
                subcontainer.AddToClassList("thunderstore-source");

                var sourceNameLabel = new Label { name = "source-name" };
                sourceNameLabel.AddToClassList("source-name");

                var packageCount = new Label() { name = "package-count"};
                var sourceHeader = new VisualElement();
                sourceHeader.AddToClassList("source-header");

                sourceHeader.Add(sourceNameLabel);
                sourceHeader.Add(packageCount);
                sourceHeader.Add(CreateStandardField("Url"));

                subcontainer.Add(sourceHeader);

                return new VisualElement { subcontainer };
            };

            sourceList.bindItem = (ve, i) =>
            {
                var source = sources[i];
                var nameLabel = ve.Q<Label>("source-name");
                var packageCount = ve.Q<Label>("package-count");
                nameLabel.text = source.Name;
                packageCount.text = $"Packages: {source.Packages.Count}";
                ve.Bind(new SerializedObject(source));
            };
            sourceList.itemsSource = sources;
            rootElement.Add(thunderstoreSettingsTemplate);
        }

        readonly string[] keywords = new string[] { nameof(ThunderstoreSource.Url) };
        public override IEnumerable<string> Keywords() => keywords;
    }
}
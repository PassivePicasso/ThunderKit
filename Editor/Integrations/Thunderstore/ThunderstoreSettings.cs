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
        const string StylePath = "Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/ThunderstoreStyle.uss";
        const string TemplatePath = "Packages/com.passivepicasso.thunderkit/Editor/Integrations/Thunderstore/ThunderstoreTemplate.uxml";

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            TemplateHelpers.MultiVersionLoadStyleSheet(rootElement, StylePath);

            var sources = AssetDatabase.FindAssets("t:PackageSource", new[] { "Assets", "Packages" })
                      .Select(AssetDatabase.GUIDToAssetPath)
                      .Select(AssetDatabase.LoadAssetAtPath<PackageSource>)
                      .OfType<ThunderstoreSource>()
                      .ToArray();
            foreach (var source in sources)
            {
                var container = new VisualElement();

                var sourceNameLabel = new Label($"{source.Name}");
                sourceNameLabel.AddToClassList("source-name");
                container.Add(sourceNameLabel);

                var urlField = CreateStandardField("Url");
                container.Add(urlField);

                rootElement.Add(container);
                container.AddToClassList("thunderstore-source");

                container.Bind(new SerializedObject(source));
            }

            var thunderstoreSettingsTemplate = TemplateHelpers.LoadTemplateInstance(TemplatePath);
            rootElement.Add(thunderstoreSettingsTemplate);
        }

        readonly string[] keywords = new string[] { nameof(ThunderstoreSource.Url) };
        public override IEnumerable<string> Keywords() => keywords;
    }
}
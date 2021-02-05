using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;
using ThunderKit.Core.Editor;
using System;
using System.Linq;
#if UNITY_2019 || UNITY_2020
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Data
{
    // Create a new type of Settings Asset.
    public class ThunderstoreSettings : ThunderKitSetting
    {
        [SerializeField]
        public string ThunderstoreUrl = "https://thunderstore.io";
        
        public static event EventHandler<(string newValue, string previousValue)> OnThunderstoreUrlChanged;

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            var settingsobject = GetOrCreateSettings<ThunderstoreSettings>();
            var serializedSettings = new SerializedObject(settingsobject);
            var container = new VisualElement();
            var label = new Label(ObjectNames.NicifyVariableName(nameof(ThunderstoreUrl)));
            var field = new TextField { bindingPath = nameof(ThunderstoreUrl) };
            field.RegisterCallback<ChangeEvent<string>>(ce =>
            {
                if (ce.newValue != ce.previousValue)
                    OnThunderstoreUrlChanged?.Invoke(field, (ce.newValue, ce.previousValue));
            });
            container.Add(label);
            container.Add(field);
            container.AddToClassList("thunderkit-field");
            field.AddToClassList("thunderkit-field-input");
            label.AddToClassList("thunderkit-field-label");
            rootElement.Add(container);

            container.Bind(serializedSettings);
        }

        public override IEnumerable<string> Keywords() => Enumerable.Empty<string>().Append(nameof(ThunderstoreUrl));
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace ThunderKit.Core.Data
{
    public class ThunderKitSetting : ScriptableObject 
    {
        public static T GetOrCreateSettings<T>() where T : ThunderKitSetting =>
            ScriptableHelper.EnsureAsset<T>($"Assets/ThunderKitSettings/{typeof(T).Name}.asset", settings => settings.Initialize());

        public virtual void Initialize() { }
        public virtual IEnumerable<string> Keywords() => Enumerable.Empty<string>();
        public virtual void CreateSettingsUI(VisualElement rootElement) { }

        protected static Type[] createSettingsUiParameterTypes = new[] { typeof(VisualElement) };

        protected static VisualElement CreateStandardField(string gamePath)
        {
            var container = new VisualElement();
            var label = new Label(ObjectNames.NicifyVariableName(gamePath));
            var field = new TextField { bindingPath = gamePath, };
            container.Add(label);
            container.Add(field);
            container.AddToClassList("thunderkit-field");
            field.AddToClassList("thunderkit-field-input");
            label.AddToClassList("thunderkit-field-label");
            return container;
        }

    }
}
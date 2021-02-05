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

        public static SerializedObject GetSerializedSettings<T>() where T : ThunderKitSetting => new SerializedObject(GetOrCreateSettings<T>());
        public virtual void Initialize() { }
        public virtual IEnumerable<string> Keywords() => Enumerable.Empty<string>();
        public virtual void CreateSettingsUI(VisualElement rootElement) { }

        protected static Type[] createSettingsUiParameterTypes = new[] { typeof(VisualElement) };

    }
}
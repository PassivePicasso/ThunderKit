using System;
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Editor;
using UnityEditor;
using UnityEngine;
using System.IO;
using ThunderKit.Core.Windows;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Data
{
    public class ThunderKitSetting : ScriptableObject
    {
        static Type[] thunderKitSettingsTypes = null;
        [InitializeOnLoadMethod]
        static void Ensure()
        {
            SettingsWindow.OnSettingsLoading -= Ensure;
            SettingsWindow.OnSettingsLoading += Ensure;

            if(thunderKitSettingsTypes == null)
                thunderKitSettingsTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(t => !t.IsAbstract && t != typeof(ThunderKitSetting) && typeof(ThunderKitSetting).IsAssignableFrom(t))
                .Distinct()
                .ToArray();

            foreach (var settingType in thunderKitSettingsTypes) 
                GetOrCreateSettings(settingType);
        }

        public static T GetOrCreateSettings<T>() where T : ThunderKitSetting
        {
            string assetPath = $"Assets/ThunderKitSettings/{typeof(T).Name}.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            return ScriptableHelper.EnsureAsset<T>(assetPath, settings => settings.Initialize());
        }
        static object GetOrCreateSettings(Type t) 
        {
            if (!typeof(ThunderKitSetting).IsAssignableFrom(t)) throw new ArgumentException($"parameter t is typeof({t.Name}), t must be assignable to typeof({typeof(ThunderKitSetting).Name}");
            string assetPath = $"Assets/ThunderKitSettings/{t.Name}.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            return ScriptableHelper.EnsureAsset(assetPath, t, obj =>
            {
                var setting = obj as ThunderKitSetting;
                setting.Initialize();
            });
        }

        public virtual void Initialize() { }
        public virtual IEnumerable<string> Keywords() => Enumerable.Empty<string>();
        public virtual void CreateSettingsUI(VisualElement rootElement) { }

        protected static Type[] createSettingsUiParameterTypes = new[] { typeof(VisualElement) };

        protected static VisualElement CreateStandardField(string gamePath)
        {
            var container = new VisualElement();
            var label = ObjectNames.NicifyVariableName(gamePath);
            var field = new PropertyField { bindingPath = gamePath, label = label };
            container.Add(field);
            container.AddToClassList("thunderkit-field");
            field.AddToClassList("thunderkit-field-input");
            return container;
        }

    }
}
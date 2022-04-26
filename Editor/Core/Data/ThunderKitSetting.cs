using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;
using ThunderKit.Core.Windows;
using ThunderKit.Core.Utilities;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Data
{
    public abstract class ThunderKitSetting : ScriptableObject
    {
        static Type[] thunderKitSettingsTypes = null;

        public virtual string DisplayName => ObjectNames.NicifyVariableName(name);

        protected Action OnChanged;

        private Editor editor;
        [InitializeOnLoadMethod]
        static void Ensure()
        {
            SettingsWindow.OnSettingsLoading -= Ensure;
            SettingsWindow.OnSettingsLoading += Ensure;

            if (thunderKitSettingsTypes == null)
                try
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var allTypes = assemblies.SelectMany(asm =>
                    {
                        try
                        {
                            return asm.GetTypes();
                        }
                        catch
                        {
                            return Array.Empty<Type>();
                        }
                    }).ToArray();
                    var concreteTypes = allTypes.Where(t => !t.IsAbstract).ToArray();
                    thunderKitSettingsTypes = concreteTypes.Where(t => typeof(ThunderKitSetting).IsAssignableFrom(t)).ToArray();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

            if (thunderKitSettingsTypes != null)
                foreach (var settingType in thunderKitSettingsTypes)
                    GetOrCreateSettings(settingType);
        }

        public static T GetOrCreateSettings<T>() where T : ThunderKitSetting
        {
            string assetPath = $"Assets/ThunderKitSettings/{typeof(T).Name}.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            return ScriptableHelper.EnsureAsset<T>(assetPath, settings => settings.Initialize());
        }

        public  static object GetOrCreateSettings(Type settingType)
        {
            var tksType = typeof(ThunderKitSetting);
            if (!tksType.IsAssignableFrom(settingType)) 
                throw new ArgumentException($"parameter t is typeof({settingType.Name}), t must be assignable to typeof({tksType.Name}");

            string assetPath = $"Assets/ThunderKitSettings/{settingType.Name}.asset";

            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            return ScriptableHelper.EnsureAsset(assetPath, settingType, obj =>
            {
                var setting = obj as ThunderKitSetting;
                setting.Initialize();
            });
        }

        public virtual void Initialize() { }
        public virtual IEnumerable<string> Keywords() => Enumerable.Empty<string>();
        public virtual void CreateSettingsUI(VisualElement rootElement)
        {
            if (!editor)
                editor = Editor.CreateEditor(this);
            var serializedObject = new SerializedObject(this);
            var imgui = new IMGUIContainer(() =>
            {
                EditorGUIUtility.labelWidth = 250;
                EditorGUI.BeginChangeCheck();
                DrawPropertiesExcluding(serializedObject, "m_Script");
                if(EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    OnChanged?.Invoke();
                }
            });
            imgui.AddToClassList("m4");
            rootElement.Add(imgui);
        }
        static void DrawPropertiesExcluding(SerializedObject obj, params string[] propertyToExclude)
        {
            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!propertyToExclude.Contains(iterator.name))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
        }
        protected static VisualElement CreateStandardField(string fieldPath)
        {
            var container = new VisualElement();
            var label = ObjectNames.NicifyVariableName(fieldPath);
            var field = new PropertyField { bindingPath = fieldPath, label = label };
            container.Add(field);
            container.AddToClassList("thunderkit-field");
            field.AddToClassList("thunderkit-field-input");
            return container;
        }

    }
}
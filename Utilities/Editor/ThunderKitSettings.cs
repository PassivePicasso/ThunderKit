#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace PassivePicasso.ThunderKit.Utilities
{
    // Create a new type of Settings Asset.
    public class ThunderKitSettings : ScriptableObject
    {
        public static string SettingsPath => $"Assets/{nameof(ThunderKitSettings)}.asset";
        private const string PathLabel = "Game Path";

        [SerializeField]
        public string GameExecutable;

        [SerializeField]
        public string GamePath;

        [SerializeField]
        public string DnSpyPath;

        [SerializeField]
        public bool Is64Bit;

        public static ThunderKitSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ThunderKitSettings>(SettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<ThunderKitSettings>();
                settings.GamePath = "";
                settings.DnSpyPath = "";
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }


        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/ThunderKit", SettingsScope.Project)
            {
                label = "ThunderKit",

                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settingsobject = GetOrCreateSettings();
                    var serializedSettings = GetSerializedSettings();

                    var pathField = new TextField { bindingPath = nameof(GameExecutable) };
                    rootElement.Add(pathField);

                    pathField = new TextField { bindingPath = nameof(GamePath) };
                    rootElement.Add(pathField);


                    //pathField = new TextField { bindingPath = nameof(DnSpyPath) };
                    //rootElement.Add(pathField);

                    rootElement.Bind(serializedSettings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { PathLabel })
            };

            return provider;
        }
    }
}
#endif
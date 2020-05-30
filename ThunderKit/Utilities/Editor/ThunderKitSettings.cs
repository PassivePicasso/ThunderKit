#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace RainOfStages.AutoConfig
{
    // Create a new type of Settings Asset.
    public class ThunderKitSettings : ScriptableObject
    {
        public static string SettingsPath => $"Assets/{nameof(ThunderKitSettings)}.asset";
        private const string RiskOfRain2PathLabel = "Game Path";
        [SerializeField]
        public string GamePath;


        //[SerializeField]
        //public string DnSpyPath;

        public static ThunderKitSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ThunderKitSettings>(SettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<ThunderKitSettings>();
                settings.GamePath = "";
                //settings.DnSpyPath = "";
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
            var provider = new SettingsProvider("Project/RoS", SettingsScope.Project)
            {
                label = "Rain of Stages",

                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = GetSerializedSettings();

                    var pathField = new TextField { bindingPath = nameof(GamePath) };

                    rootElement.Add(pathField);

                    //pathField = new TextField { bindingPath = nameof(DnSpyPath) };
                    //rootElement.Add(pathField);

                    rootElement.Bind(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { RiskOfRain2PathLabel })
            };

            return provider;
        }
    }


}
#endif
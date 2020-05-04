using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace RainOfStages.AutoConfig
{
    // Create a new type of Settings Asset.
    public class RainOfStagesSettings : ScriptableObject
    {
        public const string SettingsPath = "Assets/RainOfStagesSettings.asset";
        private const string RiskOfRain2PathLabel = "Risk of Rain 2 Path";
        [SerializeField]
        public string RoR2Path;

        internal static RainOfStagesSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<RainOfStagesSettings>(SettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<RainOfStagesSettings>();
                settings.RoR2Path = "";
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
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

                    var pathField = new TextField { bindingPath = nameof(RoR2Path) };

                    rootElement.Add(pathField);

                    rootElement.Bind(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { RiskOfRain2PathLabel })
            };

            return provider;
        }
    }


}
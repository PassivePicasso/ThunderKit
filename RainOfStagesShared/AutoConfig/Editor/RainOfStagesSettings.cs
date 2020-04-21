//using System.Collections.Generic;
//using System.IO;
//using UnityEditor;
////using UnityEditor.Experimental.UIElements;
//using UnityEngine;
//using UnityEngine.Experimental.UIElements;
//using UnityEngine.Experimental.UIElements.StyleEnums;

//namespace RainOfStages.AutoConfig
//{
//    // Create a new type of Settings Asset.
//    public class RainOfStagesSettings : ScriptableObject
//    {
//        public const string SettingsPath = "Assets/RainOfStagesSettings.asset";
//        private const string RiskOfRain2PathLabel = "Risk of Rain 2 Path";
//        [SerializeField]
//        public string RoR2Path;

//        internal static RainOfStagesSettings GetOrCreateSettings()
//        {
//            var settings = AssetDatabase.LoadAssetAtPath<RainOfStagesSettings>(SettingsPath);
//            if (settings == null)
//            {
//                settings = CreateInstance<RainOfStagesSettings>();
//                settings.RoR2Path = "";
//                AssetDatabase.CreateAsset(settings, SettingsPath);
//                AssetDatabase.SaveAssets();
//            }
//            return settings;
//        }

//        internal static SerializedObject GetSerializedSettings()
//        {
//            return new SerializedObject(GetOrCreateSettings());
//        }


//        [SettingsProvider]
//        public static SettingsProvider CreateMyCustomSettingsProvider()
//        {
//            // First parameter is the path in the Settings window.
//            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
//            var provider = new SettingsProvider("Project/RoS", SettingsScope.Project)
//            {
//                label = "Rain of Stages",
//                // activateHandler is called when the user clicks on the Settings item in the Settings window.
//                activateHandler = (searchContext, rootElement) =>
//                {
//                    var settings = GetSerializedSettings();

//                    rootElement.Add(new Label("WTF BRO!"));

//                    var sp = settings.FindProperty(nameof(RoR2Path));
//                    var pathField = new PropertyField(sp, RiskOfRain2PathLabel);

//                    rootElement.Add(pathField);
//                },

//                // Populate the search keywords to enable smart search filtering and label highlighting:
//                keywords = new HashSet<string>(new[] { RiskOfRain2PathLabel })
//            };

//            return provider;
//        }
//    }


//}
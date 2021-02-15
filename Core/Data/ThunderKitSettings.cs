using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using ThunderKit.Common;
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
    public class ThunderKitSettings : ThunderKitSetting
    {
        [InitializeOnLoadMethod]
        static void SetupPostCompilationAssemblyCopy()
        {
            CompilationPipeline.assemblyCompilationFinished -= LoadAllAssemblies;
            CompilationPipeline.assemblyCompilationFinished += LoadAllAssemblies;
            LoadAllAssemblies(null, null);
        }

        static void LoadAllAssemblies(string arg1, CompilerMessage[] arg2)
        {
            foreach (var file in Directory.EnumerateFiles("Packages", "*.dll", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(file);
                var outputPath = Path.Combine("Library", "ScriptAssemblies", fileName);
                if (File.Exists(outputPath)) File.Delete(outputPath);

                File.Copy(file, outputPath, true);
            }
        }

        [MenuItem(Constants.ThunderKitMenuRoot + "Create Settings", priority = Constants.ThunderKitMenuPriority)]
        public static void CreateSettings() => GetOrCreateSettings<ThunderKitSettings>();

        private const string PathLabel = "Game Path";

        [SerializeField]
        public string GameExecutable;

        [SerializeField]
        public string GamePath;

        [SerializeField]
        public bool Is64Bit;

        public override void Initialize() => GamePath = "";

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            var settings = GetOrCreateSettings<ThunderKitSettings>();
            var serializedSettings = new SerializedObject(settings);

            rootElement.Add(CreateStandardField(nameof(GameExecutable)));

            rootElement.Add(CreateStandardField(nameof(GamePath)));

            rootElement.Bind(serializedSettings);
        }


#if UNITY_2018 || UNITY_2019
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
                    var allTypes = AppDomain.CurrentDomain
                                .GetAssemblies()
                                .Where(asm => asm.FullName.Equals(typeof(ThunderKitSetting).Assembly.FullName)
                                           || asm.GetReferencedAssemblies().Any(reffed => reffed.FullName.Equals(typeof(ThunderKitSetting).Assembly.FullName)))
                                .SelectMany(asm => asm.GetTypes());

                    var thunderKitSettings = allTypes
                        .Where(typeof(ThunderKitSetting).IsAssignableFrom)
                        .Where(t => t != typeof(ThunderKitSetting))
                        .OrderBy(t => t.FullName)
                        .ToArray();

                    object[] createSettingsUiParameters = new[] { rootElement };
                    foreach (var settingType in thunderKitSettings)
                    {
                        var settingContainer = new VisualElement();
                        var settingsLabel = new Label(ObjectNames.NicifyVariableName(settingType.Name));
                        settingsLabel.AddToClassList("thunderkit-header");
                        settingContainer.Add(settingsLabel);
                        try
                        {
                            var getOrCreateSettings = typeof(ThunderKitSetting)
                                .GetMethod(nameof(GetOrCreateSettings), BindingFlags.Static | BindingFlags.Public)
                                .MakeGenericMethod(settingType);

                            var createSettingsUi = settingType.GetMethod(nameof(CreateSettingsUI), createSettingsUiParameterTypes);

                            var settings = getOrCreateSettings.Invoke(null, null);
                            createSettingsUiParameters[0] = settingContainer;
                            createSettingsUi.Invoke(settings, createSettingsUiParameters);
                            settingContainer.AddToClassList("thunderkit-setting");
                        }
                        catch (Exception)
                        {
                            var label = new Label($"Error encountered adding setting ui for {settingType.FullName}");
                            settingContainer.Insert(0, label);
                            settingContainer.AddToClassList("thunderkit-error");
                        }
                        rootElement.Add(settingContainer);
                    }

                    
                    //var localPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)
#if UNITY_2018
                    var ussPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:stylesheet ThunderKitSettings2018").First());
                    rootElement.AddStyleSheetPath(ussPath);
#elif UNITY_2019
                    var ussPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:stylesheet ThunderKitSettings2019").First());
                    var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
                    rootElement.styleSheets.Add(styleSheet);
#endif
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { PathLabel })
            };

            return provider;
        }
#endif

                }
}
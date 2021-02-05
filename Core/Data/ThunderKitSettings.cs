using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;
using ThunderKit.Core.Editor;
using System;
using System.Linq;
using System.Reflection;
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
                    ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
                    var serializedSettings = GetSerializedSettings<ThunderKitSettings>();

                    var label = new Label(ObjectNames.NicifyVariableName(nameof(GameExecutable)));
                    var field = new TextField { bindingPath = nameof(GameExecutable) };
                    rootElement.Add(label);
                    rootElement.Add(field);

                    label = new Label(ObjectNames.NicifyVariableName(nameof(GamePath)));
                    field = new TextField { bindingPath = nameof(GamePath), };
                    rootElement.Add(label);
                    rootElement.Add(field);
                    rootElement.Bind(serializedSettings);

                    var allTypes = AppDomain.CurrentDomain
                                .GetAssemblies()
                                .Where(asm => asm.GetReferencedAssemblies().Any(reffed => reffed.Name.Contains("ThunderKit")))
                                .SelectMany(asm => asm.GetTypes());
                    var thunderKitSettings = allTypes
                        .Where(typeof(ThunderKitSetting).IsAssignableFrom)
                        .Where(t => t != typeof(ThunderKitSettings))
                        .Where(t => t != typeof(ThunderKitSetting))
                        .ToArray();


                    object[] createSettingsUiParameters = new[] { rootElement };
                    foreach (var settingType in thunderKitSettings)
                    {
                        var getOrCreateSettings = typeof(ThunderKitSetting)
                                .GetMethod(nameof(GetOrCreateSettings), BindingFlags.Static | BindingFlags.Public)
                                .MakeGenericMethod(settingType);

                        var createSettingsUi = settingType.GetMethod(nameof(CreateSettingsUI), createSettingsUiParameterTypes);

                        var settings = getOrCreateSettings.Invoke(null, null);
                        createSettingsUi.Invoke(settings, createSettingsUiParameters);
                    }
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { PathLabel })
            };

            return provider;
        }
#endif

    }
}
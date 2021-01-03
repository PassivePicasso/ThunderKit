#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;
using PassivePicasso.ThunderKit.Editor;
#if UNITY_2019 || UNITY_2020
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace PassivePicasso.ThunderKit.Data
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
        public string[] assembly_metadata;

        [SerializeField]
        public string[] additional_plugins;

        [SerializeField]
        public string[] additional_assemblies;

        [SerializeField]
        public string[] excluded_assemblies;

        [SerializeField]
        public string[] deployment_exclusions;


        [SerializeField]
        public bool Is64Bit;

        [InitializeOnLoadMethod]
        static void SetupPostCompilationAssemblyCopy()
        {
            CompilationPipeline.assemblyCompilationFinished -= CopyAssemblyCSharp;
            CompilationPipeline.assemblyCompilationFinished += CopyAssemblyCSharp;
            CopyAssemblyCSharp(null, null);
        }

        static void CopyAssemblyCSharp(string arg1, CompilerMessage[] arg2)
        {
            CopyAssembly("Assembly-CSharp-firstpass.dll");
            CopyAssembly("Assembly-CSharp.dll");
        }

        private static void CopyAssembly(string fileName)
        {
            var settings = ThunderKitSettings.GetOrCreateSettings();
            var packageName = Path.GetFileNameWithoutExtension(settings.GameExecutable);
            var filePath = Path.Combine("Packages", packageName, fileName);
            var outputPath = Path.Combine("Library", "ScriptAssemblies", fileName);
            if (File.Exists(filePath))
                File.Copy(filePath, outputPath, true);
        }

        public static ThunderKitSettings GetOrCreateSettings() =>
            ScriptableHelper.EnsureAsset<ThunderKitSettings>(SettingsPath, settings =>
            {
                settings.GamePath = "";
                settings.deployment_exclusions = new string[0];
            });

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        [MenuItem(ScriptableHelper.ThunderKitMenuRoot + "Create Settings")]
        public static void CreateSettings()
        {
            GetOrCreateSettings();
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
#endif

    }
}
#endif
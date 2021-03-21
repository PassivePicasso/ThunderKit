using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using ThunderKit.Common;
using ThunderKit.Core.Config;
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

        private static string[] CopyFilePatterns = new[] { "*.dll", "*.mdb", "*.pdb" };
        static void LoadAllAssemblies(string somevalue, CompilerMessage[] message)
        {
            var targetFiles = from pattern in CopyFilePatterns
                              from file in Directory.EnumerateFiles("Packages", pattern, SearchOption.AllDirectories)
                              select file;
            foreach (var file in targetFiles)
            {
                var fileName = Path.GetFileName(file);
                var outputPath = Path.Combine("Library", "ScriptAssemblies", fileName);
                if (File.Exists(outputPath)) File.Delete(outputPath);

                File.Copy(file, outputPath, true);
            }
        }


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

            var configureButton = new Button(() => ConfigureGame.Configure());
            configureButton.AddToClassList("configure-game-button");
            configureButton.text = "Locate Game";
            rootElement.Add(configureButton);

            rootElement.Bind(serializedSettings);
        }
    }
}
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
using ThunderKit.Markdown;
using ThunderKit.Core.Windows;
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
            EditorApplication.wantsToQuit -= EditorApplication_wantsToQuit;
            EditorApplication.wantsToQuit += EditorApplication_wantsToQuit;

            var settings = GetOrCreateSettings<ThunderKitSettings>();
            if (settings.FirstLoad && settings.ShowOnStartup)
                EditorApplication.update += ShowSettings;
        }

        private static void ShowSettings()
        {
            EditorApplication.update -= ShowSettings;
            var window = EditorWindow.GetWindow<Settings>();
            Settings.ShowSettings();
            var settings = GetOrCreateSettings<ThunderKitSettings>();
            settings.FirstLoad = false;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private static bool EditorApplication_wantsToQuit()
        {
            var settings = GetOrCreateSettings<ThunderKitSettings>();
            settings.FirstLoad = true;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return true;
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

        [SerializeField]
        private bool FirstLoad = true;

        [SerializeField]
        public bool ShowOnStartup = true;

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
            MarkdownElement markdown = null;
            if (string.IsNullOrEmpty(GameExecutable) || string.IsNullOrEmpty(GamePath))
            {
                markdown = new MarkdownElement
                {
                    Data =
$@"
**_Warning:_**   No game configured, click locate game to setup your ThunderKit Project

_Uncheck Show On Startup to not show this window on next startup_
",
                    MarkdownDataType = MarkdownDataType.Text
                };
#if UNITY_2018
                markdown.AddStyleSheetPath("Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss");
#else
                markdown.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.passivepicasso.thunderkit/Documentation/uss/markdown.uss"));
#endif
                markdown.RefreshContent();
                rootElement.Add(markdown);
            }

            var child = CreateStandardField(nameof(ShowOnStartup));
            child.tooltip = "Uncheck this to stop showing this window on startup";
            rootElement.Add(child);

            rootElement.Add(CreateStandardField(nameof(GameExecutable)));

            rootElement.Add(CreateStandardField(nameof(GamePath)));

            var configureButton = new Button(() =>
            {
                ConfigureGame.Configure();
                if (!string.IsNullOrEmpty(GameExecutable) && !string.IsNullOrEmpty(GamePath))
                {
                    if (markdown != null)
                        markdown.RemoveFromHierarchy();
                }

            });
            configureButton.AddToClassList("configure-game-button");
            configureButton.text = "Locate Game";
            rootElement.Add(configureButton);

            rootElement.Bind(serializedSettings);
        }
    }
}
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;
using System.Linq;
using ThunderKit.Common;
using ThunderKit.Core.Windows;
using ThunderKit.Core.UIElements;
using ThunderKit.Core.Config;
using ThunderKit.Markdown;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Data
{
    using static ThunderKit.Common.PathExtensions;
    // Create a new type of Settings Asset.
    public class ThunderKitSettings : ThunderKitSetting
    {

        [InitializeOnLoadMethod]
        static void SetupPostCompilationAssemblyCopy()
        {
            CompilationPipeline.assemblyCompilationFinished -= LoadAllAssemblies;
            CompilationPipeline.assemblyCompilationFinished += LoadAllAssemblies;
            LoadAllAssemblies(null, null);
            GetOrCreateSettings<ThunderKitSettings>();
        }

        public override string DisplayName => "ThunderKit Settings";

        private static readonly string[] CopyFilePatterns = new[] { "*.dll", "*.mdb", "*.pdb" };
        static void LoadAllAssemblies(string somevalue, CompilerMessage[] message)
        {
            var targetFiles = from pattern in CopyFilePatterns
                              from file in Directory.GetFiles("Packages", pattern, SearchOption.AllDirectories)
                              select file;
            foreach (var file in targetFiles)
            {
                var fileName = Path.GetFileName(file);
                var outputPath = Combine("Library", "ScriptAssemblies", fileName);

                FileUtil.ReplaceFile(file, outputPath);
            }
        }
        private SerializedObject thunderKitSettingsSO;

        [SerializeField]
        private bool FirstLoad = true;

        public bool ShowOnStartup = true;

        public string GameExecutable;

        public string GamePath;

        public bool Is64Bit;

        public string DateTimeFormat = "HH:mm:ss:fff";

        public string CreatedDateFormat = "MMM/dd HH:mm:ss";

        [InitializeOnLoadMethod]
        static void SettingsWindowSetup()
        {
            EditorApplication.wantsToQuit -= EditorApplication_wantsToQuit;
            EditorApplication.wantsToQuit += EditorApplication_wantsToQuit;

            var settings = GetOrCreateSettings<ThunderKitSettings>();
            if (settings.FirstLoad && settings.ShowOnStartup)
                EditorApplication.update += ShowSettings;
        }

        private static void ShowSettings()
        {
            EditorApplication.update -= ShowSettings;
            SettingsWindow.ShowSettings();
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
        public override void Initialize() => GamePath = "";

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            MarkdownElement markdown = null;
            if (string.IsNullOrEmpty(GameExecutable) || string.IsNullOrEmpty(GamePath))
            {
                markdown = new MarkdownElement
                {
                    Data =
$@"
            **_Warning:_**   No game configured. Click the Locate Game button to setup your ThunderKit Project before continuing
            ",
                    MarkdownDataType = MarkdownDataType.Text
                };

                markdown.AddToClassList("m4");
                markdown.RefreshContent();
                rootElement.Add(markdown);
            }
            var settingsElement = TemplateHelpers.LoadTemplateInstance(Constants.ThunderKitSettingsTemplatePath);
            rootElement.Add(settingsElement);

            var browseButton = settingsElement.Q<Button>("browse-button");
            browseButton.clickable.clicked += () =>
            {
                ConfigureGame.LocateGame(this);
                if (!string.IsNullOrEmpty(GameExecutable) && !string.IsNullOrEmpty(GamePath))
                {
                    if (markdown != null)
                        markdown.RemoveFromHierarchy();
                }
            };
            var loadButton = settingsElement.Q<Button>("load-button");
            loadButton.clickable.clicked += () =>
            {
                ConfigureGame.LoadGame(this);
            };

            if (thunderKitSettingsSO == null)
                thunderKitSettingsSO = new SerializedObject(this);

            rootElement.Bind(thunderKitSettingsSO);
        }
    }
}
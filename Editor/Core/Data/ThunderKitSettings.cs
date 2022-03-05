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
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Manifests;
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
            EditorApplication.quitting -= EditorApplicationQuitting;
            EditorApplication.quitting += EditorApplicationQuitting;

            CompilationPipeline.assemblyCompilationFinished -= CopyAssemblyCSharp;
            CompilationPipeline.assemblyCompilationFinished += CopyAssemblyCSharp;

            GetOrCreateSettings<ThunderKitSettings>();
        }

        public override string DisplayName => "ThunderKit Settings";
        private static void EditorApplicationQuitting()
        {
            CopyAssemblyCSharp(null, null);
        }

        static void CopyAssemblyCSharp(string somevalue, CompilerMessage[] message)
        {
            foreach (var file in Directory.GetFiles("Packages", "Assembly-CSharp.dll", SearchOption.AllDirectories))
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

        public string GameDataPath => Path.Combine(GamePath, $"{Path.GetFileNameWithoutExtension(GameExecutable)}_Data");

        public string StreamingAssetsPath => Path.Combine(GameDataPath, "StreamingAssets");

        public int IncludedSettings;

        public bool Is64Bit;

        public string DateTimeFormat = "HH:mm:ss:fff";

        public string CreatedDateFormat = "MMM/dd HH:mm:ss";

        public bool ShowLogWindow = true;

        public Pipeline[] QuickAccessPipelines;
        public string[] QuickAccessPipelineNames;
        public Manifest[] QuickAccessManifests;
        public string[] QuickAccessManifestNames;

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


        public void SetQuickAccess(Pipeline pipeline, bool quickAccess)
        {
            if (quickAccess)
                QuickAccessPipelines = (QuickAccessPipelines ?? System.Array.Empty<Pipeline>()).Append(pipeline).OrderBy(m => m.name).ToArray();
            else
                QuickAccessPipelines = (QuickAccessPipelines ?? System.Array.Empty<Pipeline>()).Where(m => m.name != pipeline.name).OrderBy(name => name).ToArray();

            QuickAccessPipelineNames = QuickAccessPipelines.Select(m => m.name).OrderBy(name => name).ToArray();
        }
        public void SetQuickAccess(Manifest manifest, bool quickAccess)
        {
            if (quickAccess)
                QuickAccessManifests = (QuickAccessManifests ?? System.Array.Empty<Manifest>()).Append(manifest).OrderBy(m => m.name).ToArray();
            else
                QuickAccessManifests = (QuickAccessManifests ?? System.Array.Empty<Manifest>()).Where(m => m.name != manifest.name).OrderBy(name => name).ToArray();

            QuickAccessManifestNames = QuickAccessManifests.Select(m => m.name).OrderBy(name => name).ToArray();
        }
    }
}
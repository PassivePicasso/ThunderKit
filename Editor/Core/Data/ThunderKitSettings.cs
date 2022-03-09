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
using ThunderKit.Core.Utilities;
using System.Collections.Generic;
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
        public enum GuidMode { Original, Stabilized, AssetRipperCompatibility }
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

        public string ManagedAssembliesPath => Path.Combine(GameDataPath, $"Managed");

        public string StreamingAssetsPath => Path.Combine(GameDataPath, "StreamingAssets");

        public string PackageName
        {
            get
            {
                var gameName = Path.GetFileNameWithoutExtension(GameExecutable);
                return gameName.ToLower().Split(' ').Aggregate((a, b) => $"{a}{b}");
            }
        }
        public string PackagePath => $"Packages/{PackageName}";
        public string PackagePluginsPath => $"Packages/{PackageName}/plugins";

        public int IncludedSettings;

        public bool Is64Bit;

        public string DateTimeFormat = "HH:mm:ss:fff";

        public string CreatedDateFormat = "MMM/dd HH:mm:ss";

        public bool ShowLogWindow = true;

        public Pipeline SelectedPipeline;
        public Manifest SelectedManifest;
        public Pipeline[] QuickAccessPipelines;
        public Manifest[] QuickAccessManifests;
        public string[] QuickAccessPipelineNames;
        public string[] QuickAccessManifestNames;

        public GuidMode OldGuidGenerationMode = GuidMode.Original;
        public GuidMode GuidGenerationMode = GuidMode.Original;
        private MarkdownElement markdown;

        [InitializeOnLoadMethod]
        static void SettingsWindowSetup()
        {
            EditorApplication.wantsToQuit -= EditorApplication_wantsToQuit;
            EditorApplication.wantsToQuit += EditorApplication_wantsToQuit;

            var settings = GetOrCreateSettings<ThunderKitSettings>();
            if (settings.FirstLoad && settings.ShowOnStartup)
                EditorApplication.update += ShowSettings;

            EditorApplication.projectChanged += EditorApplication_projectChanged;
            settings.QuickAccessPipelines = AssetDatabase.FindAssets($"t:{nameof(Pipeline)}", Constants.FindAllFolders)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Pipeline>(path))
                .Where(pipeline => pipeline.QuickAccess)
                .ToArray();
            settings.QuickAccessManifests = AssetDatabase.FindAssets($"t:{nameof(Manifest)}", Constants.FindAllFolders)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Manifest>(path))
                .Where(manifest => manifest.QuickAccess)
                .ToArray();
            settings.QuickAccessPipelineNames = settings.QuickAccessPipelines.Where(a => a).Select(m => m.name).OrderBy(name => name).ToArray();
            settings.QuickAccessManifestNames = settings.QuickAccessManifests.Where(a => a).Select(m => m.name).OrderBy(name => name).ToArray();
        }

        private static void EditorApplication_projectChanged()
        {
            var settings = GetOrCreateSettings<ThunderKitSettings>();
            settings.QuickAccessPipelineNames = settings.QuickAccessPipelines.Where(a => a).Select(m => m.name).OrderBy(name => name).ToArray();
            settings.QuickAccessManifestNames = settings.QuickAccessManifests.Where(a => a).Select(m => m.name).OrderBy(name => name).ToArray();
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
            if (string.IsNullOrEmpty(GameExecutable) || string.IsNullOrEmpty(GamePath))
            {
                markdown = new MarkdownElement
                {
                    Data = $"{Constants.ThunderKitRoot}/UXML/Settings/ThunderKitSettingsWarning.md",
                    MarkdownDataType = MarkdownDataType.Source
                };

                markdown.AddToClassList("m4");
                markdown.RefreshContent();
                rootElement.Add(markdown);
            }

            var settingsElement = TemplateHelpers.LoadTemplateInstance(Constants.ThunderKitSettingsTemplatePath);
            settingsElement.AddEnvironmentAwareSheets(Constants.ThunderKitSettingsTemplatePath);

            rootElement.Add(settingsElement);

            var guidGenerationModeField = settingsElement.Q<EnumField>("guid-mode-field");
#if UNITY_2019_1_OR_NEWER
            guidGenerationModeField.RegisterValueChangedCallback(OnGuidChanged);
#elif UNITY_2018_1_OR_NEWER
            guidGenerationModeField.OnValueChanged(OnGuidChanged);
#endif
            guidGenerationModeField.value = GuidGenerationMode;

            var browseButton = settingsElement.Q<Button>("browse-button");
            browseButton.clickable.clicked -= BrowserForGame;
            browseButton.clickable.clicked += BrowserForGame;

            var loadButton = settingsElement.Q<Button>("load-button");
            loadButton.clickable.clicked -= LoadGame;
            loadButton.clickable.clicked += LoadGame;

            var updateButton = settingsElement.Q<Button>("update-button");
            updateButton.clickable.clicked -= UpdateGuids;
            updateButton.clickable.clicked += UpdateGuids;

            if (thunderKitSettingsSO == null)
                thunderKitSettingsSO = new SerializedObject(this);

            rootElement.Bind(thunderKitSettingsSO);
        }

        void OnGuidChanged(ChangeEvent<System.Enum> evt)
        {
            var guidMode = (GuidMode)evt.newValue;
            GuidGenerationMode = guidMode;
        }

        private void LoadGame()
        {
            ConfigureGame.LoadGame(this);
            OldGuidGenerationMode = GuidGenerationMode;
        }

        private void BrowserForGame()
        {
            ConfigureGame.LocateGame(this);
            if (!string.IsNullOrEmpty(GameExecutable) && !string.IsNullOrEmpty(GamePath))
            {
                if (markdown != null)
                    markdown.RemoveFromHierarchy();
            }

        }

        private void UpdateGuids()
        {
            string nativeAssemblyExtension = string.Empty;

            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                    nativeAssemblyExtension = "dylib";
                    break;
                case RuntimePlatform.WindowsEditor:
                    nativeAssemblyExtension = "dll";
                    break;
                case RuntimePlatform.LinuxEditor:
                    nativeAssemblyExtension = "so";
                    break;
            }
            Dictionary<string, string> guidMaps = new Dictionary<string, string>();

            foreach (var installedAssembly in Directory.EnumerateFiles(PackagePath, $"*.dll", SearchOption.TopDirectoryOnly))
            {
                var asmPath = installedAssembly.Replace("\\", "/");
                string assemblyFileName = Path.GetFileName(asmPath);
                var destinationMetaData = Combine(PackagePath, $"{assemblyFileName}.meta");
                guidMaps[PackageHelper.GetFileNameHash(assemblyFileName, OldGuidGenerationMode)] = PackageHelper.GetFileNameHash(assemblyFileName, GuidGenerationMode);
                PackageHelper.WriteAssemblyMetaData(asmPath, destinationMetaData);
            }
            foreach (var installedAssembly in Directory.EnumerateFiles(PackagePluginsPath, $"*.{nativeAssemblyExtension}", SearchOption.TopDirectoryOnly))
            {
                var asmPath = installedAssembly.Replace("\\", "/");
                string assemblyFileName = Path.GetFileName(asmPath);
                var destinationMetaData = Combine(PackagePluginsPath, $"{assemblyFileName}.meta");
                guidMaps[PackageHelper.GetFileNameHash(assemblyFileName, OldGuidGenerationMode)] = PackageHelper.GetFileNameHash(assemblyFileName, GuidGenerationMode);
                PackageHelper.WriteAssemblyMetaData(asmPath, destinationMetaData);
            }
            OldGuidGenerationMode = GuidGenerationMode;
            new SerializedObject(this).ApplyModifiedProperties();
            AssetDatabase.Refresh();
        }

        public void SetQuickAccess(Pipeline pipeline, bool quickAccess)
        {
            if (quickAccess)
                QuickAccessPipelines = (QuickAccessPipelines ?? System.Array.Empty<Pipeline>()).Where(a => a).Append(pipeline).OrderBy(m => m.name).ToArray();
            else
                QuickAccessPipelines = (QuickAccessPipelines ?? System.Array.Empty<Pipeline>()).Where(a => a).Where(m => m.name != pipeline.name).OrderBy(name => name).ToArray();

            QuickAccessPipelineNames = QuickAccessPipelines.Select(m => m.name).OrderBy(name => name).ToArray();
        }
        public void SetQuickAccess(Manifest manifest, bool quickAccess)
        {
            if (quickAccess)
                QuickAccessManifests = (QuickAccessManifests ?? System.Array.Empty<Manifest>()).Where(a => a).Append(manifest).OrderBy(m => m.name).ToArray();
            else
                QuickAccessManifests = (QuickAccessManifests ?? System.Array.Empty<Manifest>()).Where(a => a).Where(m => m.name != manifest.name).OrderBy(name => name).ToArray();

            QuickAccessManifestNames = QuickAccessManifests.Select(m => m.name).OrderBy(name => name).ToArray();
        }
    }
}
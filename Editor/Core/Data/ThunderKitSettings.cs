using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.IO;
using System.Linq;
using ThunderKit.Common;
using ThunderKit.Core.Windows;
using ThunderKit.Core.UIElements;
using ThunderKit.Markdown;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Manifests;
using System.Reflection;
using System;
using ThunderKit.Markdown.Helpers;
using System.Net;
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

    [ExecuteAlways]
    // Create a new type of Settings Asset.
    public class ThunderKitSettings : ThunderKitSetting
    {
        private const BindingFlags publicInstanceBinding = BindingFlags.Public | BindingFlags.Instance;

        [InitializeOnLoadMethod]
        static void SetupPostCompilationAssemblyCopy()
        {
            EditorApplication.quitting -= EditorApplicationQuitting;
            EditorApplication.quitting += EditorApplicationQuitting;
            EditorApplication.wantsToQuit -= EditorApplication_wantsToQuit;
            EditorApplication.wantsToQuit += EditorApplication_wantsToQuit;
            CompilationPipeline.assemblyCompilationFinished -= CopyAssemblyCSharp;
            CompilationPipeline.assemblyCompilationFinished += CopyAssemblyCSharp;

            var settings = GetOrCreateSettings<ThunderKitSettings>();
            if (settings.FirstLoad && settings.ShowOnStartup)
                EditorApplication.update += ShowSettings;

            settings.QuickAccessPipelines = AssetDatabase.FindAssets($"t:{nameof(Pipeline)}", Constants.FindAllFolders)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Pipeline>(path))
                .Where(pipeline => pipeline)
                .Where(pipeline => pipeline.QuickAccess)
                .ToArray();
            settings.QuickAccessManifests = AssetDatabase.FindAssets($"t:{nameof(Manifest)}", Constants.FindAllFolders)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Manifest>(path))
                .Where(manifest => manifest)
                .Where(manifest => manifest.QuickAccess)
                .ToArray();
            ImageElementFactory.CachePath = settings.ImageCachePath;

            ImageElementFactory.CacheUpdated += ImageElementFactory_CacheUpdated;
            ImageElementFactory_CacheUpdated(null, EventArgs.Empty);
        }

        private static void ImageElementFactory_CacheUpdated(object sender, EventArgs e)
        {
            var settings = GetOrCreateSettings<ThunderKitSettings>();
            settings.CachedImageCount = ImageElementFactory.Count;
            var realSize = ImageElementFactory.Size / (1024f * 1024f);
            var grownSize = realSize * 100;
            var truncatedGrownSize = (double)(int)grownSize;
            var truncatedSize = truncatedGrownSize / 100;
            settings.CacheSize = truncatedSize;
            EditorUtility.SetDirty(settings);
        }

        private static void EditorApplicationQuitting() => CopyAssemblyCSharp(null, null);
        private static bool EditorApplication_wantsToQuit()
        {
            var settings = GetOrCreateSettings<ThunderKitSettings>();
            settings.FirstLoad = true;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return true;
        }
        private static void CopyAssemblyCSharp(string somevalue, CompilerMessage[] message)
        {
            foreach (var file in Directory.GetFiles("Packages", "Assembly-CSharp.dll", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(file);
                var outputPath = Combine("Library", "ScriptAssemblies", fileName);

                FileUtil.ReplaceFile(file, outputPath);
            }
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
        #region Properties
        public override string DisplayName => "ThunderKit Settings";
        public string GameDataPath => Path.Combine(GamePath, $"{Path.GetFileNameWithoutExtension(GameExecutable)}_Data");
        public string ManagedAssembliesPath => Path.Combine(GameDataPath, $"Managed");
        public string StreamingAssetsPath => Path.Combine(GameDataPath, "StreamingAssets");
        public string AddressableAssetsPath => Path.Combine(StreamingAssetsPath, "aa");
        public string AddressableAssetsCatalog => Path.Combine(AddressableAssetsPath, "catalog.json");
        public string AddressableAssetsSettings => Path.Combine(AddressableAssetsPath, "settings.json");
        public static string EditTimePath
        {
            get
            {
                var settings = GetOrCreateSettings<ThunderKitSettings>();
                return settings.AddressableAssetsPath;
            }
        }
        public string PackageName
        {
            get
            {
                var gameName = Path.GetFileNameWithoutExtension(GameExecutable);
                return gameName.ToLower().Split(' ').Aggregate((a, b) => $"{a}{b}");
            }
        }
        public string PackagePath => $"Packages/{PackageName}";
        public string PackageFilePath => $"Packages/{Path.GetFileNameWithoutExtension(GameExecutable)}";
        public string PackagePluginsPath => $"{PackagePath}/plugins";
        #endregion
        #region Fields
        [SerializeField]
        private bool FirstLoad = true;
        public bool ShowOnStartup = true;
        public string GameExecutable;
        public string GamePath;
        public bool Is64Bit;
        public string DateTimeFormat = "HH:mm:ss:fff";
        public string CreatedDateFormat = "MMM/dd HH:mm:ss";
        public bool ShowLogWindow = true;
        public bool LogPackageSourceTimings;
        public string ImageCachePath = "Library/MarkdownImageCache";
        public int CachedImageCount;
        public double CacheSize;
        public MarkdownOpenMode MarkdownOpenMode = MarkdownOpenMode.UnityExternalEditor;

        public Pipeline SelectedPipeline;
        public Manifest SelectedManifest;
        public Pipeline[] QuickAccessPipelines;
        public Manifest[] QuickAccessManifests;
        private MarkdownElement markdown;
        private SerializedObject thunderKitSettingsSO;
        public ImportConfiguration ImportConfiguration;
        private Label logCountLabel;

        #endregion

        public override void Initialize()
        {
            GamePath = "";
            ImageElementFactory.CachePath = ImageCachePath;
        }


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

            var editorModeField = settingsElement.Q<EnumField>("editor-mode-field");
#if UNITY_2019_1_OR_NEWER
            editorModeField.RegisterValueChangedCallback(OnEditorModeChanged);
#elif UNITY_2018_1_OR_NEWER
            editorModeField.OnValueChanged(OnEditorModeChanged);
#endif
            editorModeField.value = MarkdownOpenMode;


            var browseButton = settingsElement.Q<Button>("browse-button");
            browseButton.clickable.clicked -= BrowseForGame;
            browseButton.clickable.clicked += BrowseForGame;

            var loadButton = settingsElement.Q<Button>("load-button");
            loadButton.clickable.clicked -= LoadGame;
            loadButton.clickable.clicked += LoadGame;


            var cacheBrowseButton = settingsElement.Q<Button>("cache-browse-button");
            cacheBrowseButton.clickable.clicked -= BrowserForCacheFolder;
            cacheBrowseButton.clickable.clicked += BrowserForCacheFolder;

            var clearCacheButton = settingsElement.Q<Button>("clear-cache-button");
            clearCacheButton.clickable.clicked -= ClearCache;
            clearCacheButton.clickable.clicked += ClearCache;

            logCountLabel = settingsElement.Q<Label>("log-count-label");
            var logCount = AssetDatabase.FindAssets("t:PipelineLog").Length;
            logCountLabel.text = $"{logCount}";
            var clearLogsButton = settingsElement.Q<Button>("clear-logs-button");
            clearLogsButton.clickable.clicked -= ClearLogCache;
            clearLogsButton.clickable.clicked += ClearLogCache;

            if (thunderKitSettingsSO == null)
                thunderKitSettingsSO = new SerializedObject(this);

            rootElement.Bind(thunderKitSettingsSO);
        }

        private void ClearLogCache()
        {
            var logs = AssetDatabase.FindAssets("t:PipelineLog")
                                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                                    .ToArray();
            var remaining = new List<string>();
            if (AssetDatabase.DeleteAssets(logs, remaining) && remaining.Count > 0)
                Debug.Log(remaining.Aggregate("Some logs were not deleted\r\n", (a, b) => $"{a}\r\n{b}"));
            logCountLabel.text = $"{0}";
        }

        private void ClearCache()
        {
            CachedImageCount = 0;
            ImageElementFactory.ClearCache();
        }

        private void BrowserForCacheFolder()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsEditor:
                    var path = EditorUtility.OpenFolderPanel("Select Cache Location", ImageCachePath, "MarkdownImageCahe");
                    if (string.IsNullOrEmpty(path)) return;

                    string currentDir = Directory.GetCurrentDirectory().Replace("\\", "/");
                    if (path.StartsWith(currentDir))
                        path = path.Substring(currentDir.Length).TrimStart('/');

                    ImageCachePath = path;
                    ImageElementFactory.CachePath = ImageCachePath;
                    EditorUtility.SetDirty(this);
                    break;
                //case RuntimePlatform.OSXEditor:
                //    path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "app");
                //    break;
                default:
                    EditorUtility.DisplayDialog("Unsupported", "Your operating system is partially or completely unsupported. Contributions to improve this are welcome", "Ok");
                    return;
            }
        }

        void OnEditorModeChanged(ChangeEvent<Enum> evt)
        {
            var openMode = (MarkdownOpenMode)evt.newValue;
            MarkdownOpenMode = openMode;
        }

        private void LoadGame()
        {
            if (!ImportConfiguration)
                ImportConfiguration = GetOrCreateSettings<ImportConfiguration>();

            ImportConfiguration.ConfigurationIndex = 0;
            ImportConfiguration.ImportGame();
        }
        private void BrowseForGame()
        {
            ImportConfiguration.LocateGame(this);
            if (!string.IsNullOrEmpty(GameExecutable) && !string.IsNullOrEmpty(GamePath))
            {
                if (markdown != null)
                    markdown.RemoveFromHierarchy();
            }
        }

        public void SetQuickAccess(Pipeline pipeline, bool quickAccess) => SetQuickAccess(pipeline, ref QuickAccessPipelines, quickAccess);
        public void SetQuickAccess(Manifest manifest, bool quickAccess) => SetQuickAccess(manifest, ref QuickAccessManifests, quickAccess);

        void SetQuickAccess<T>(T quickAccessObject, ref T[] quickAccessObjects, bool quickAccess) where T : ComposableObject
        {
            var enumerableQAO = (quickAccessObjects ?? Enumerable.Empty<T>()).Where(a => a);

            if (quickAccess)
                enumerableQAO = enumerableQAO.Append(quickAccessObject).ToArray();
            else
                enumerableQAO = enumerableQAO.Where(qao => qao != quickAccessObject).ToArray();

            quickAccessObjects = enumerableQAO.OrderBy(qao => qao.name).Distinct().ToArray();
        }

    }
}
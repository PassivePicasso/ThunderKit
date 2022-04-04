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
using System.Reflection;
using System;
using System.Text;
using ThunderKit.Core.Config.Common;
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
                .Where(pipeline => pipeline.QuickAccess)
                .ToArray();
            settings.QuickAccessManifests = AssetDatabase.FindAssets($"t:{nameof(Manifest)}", Constants.FindAllFolders)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<Manifest>(path))
                .Where(manifest => manifest.QuickAccess)
                .ToArray();

            EditorApplication.update += SetupConfigurators;
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

        private static IEnumerable<System.Reflection.Assembly> configurationAssemblies;

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

        [SerializeField]
        private bool FirstLoad = true;
        public bool ShowOnStartup = true;
        public string GameExecutable;
        public string GamePath;
        public int IncludedSettings;
        public bool Is64Bit;
        public string DateTimeFormat = "HH:mm:ss:fff";
        public string CreatedDateFormat = "MMM/dd HH:mm:ss";
        public bool ShowLogWindow = true;

        public Pipeline SelectedPipeline;
        public Manifest SelectedManifest;
        public Pipeline[] QuickAccessPipelines;
        public Manifest[] QuickAccessManifests;
        public Executor[] ConfigurationExecutors;
        private MarkdownElement markdown;
        private SerializedObject thunderKitSettingsSO;

        public override void Initialize()
        {
            GamePath = "";
        }


        private static void SetupConfigurators()
        {
            var settings = GetOrCreateSettings<ThunderKitSettings>();
            if (EditorApplication.isUpdating) return;

            var builder = new StringBuilder("Loaded GameConfigurators:");
            builder.AppendLine();
            if (configurationAssemblies == null)
                configurationAssemblies = AppDomain.CurrentDomain
                                .GetAssemblies()
                                .Where(asm => asm != null)
                                .Where(asm => asm.GetCustomAttribute<GameConfiguratorAssemblyAttribute>() != null);

            var loadedTypes = configurationAssemblies
#if NET_4_6
                .Where(asm => !asm.IsDynamic)
#else
                .Where(asm =>
                {
                    if (asm.ManifestModule is System.Reflection.Emit.ModuleBuilder mb)
                        return !mb.IsTransient();

                    return true;
                })
#endif
               .SelectMany(asm =>
               {
                   try
                   {
                       return asm.GetTypes();
                   }
                   catch (ReflectionTypeLoadException e)
                   {
                       return e.Types;
                   }
               })
               .Where(t => t != null)
               .Where(t => !t.IsAbstract && !t.IsInterface)
               .ToArray();


            var settingsPath = AssetDatabase.GetAssetPath(settings);
            var objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(settingsPath);
            var existingExecutors = new HashSet<Executor>(objects.OfType<Executor>());

            int currentExecutorCount = existingExecutors.Count;

            var executors = loadedTypes.Where(t => typeof(Executor).IsAssignableFrom(t))
                .Where(t => !existingExecutors.Any(executor => executor.GetType() == t))
                .Select(t =>
                {
                    if (existingExecutors.Any(gc => gc.GetType() == t))
                        return null;

                    var configurator = CreateInstance(t) as Executor;
                    if (configurator)
                    {
                        configurator.name = configurator.Name;
                        builder.AppendLine(configurator.GetType().AssemblyQualifiedName);
                    }
                    return configurator;
                })
                .Where(configurator => configurator != null)
                .Union(existingExecutors)
                .Distinct()
                .OrderByDescending(configurator => configurator.Priority)
                .ToList();

            foreach (var element in executors)
                if (AssetDatabase.GetAssetPath(element) != settingsPath)
                    AssetDatabase.AddObjectToAsset(element, settingsPath);

            var updatedExectors = executors.ToArray();
            settings.ConfigurationExecutors = updatedExectors;
            if (currentExecutorCount != updatedExectors.Length)
            {
                AssetDatabase.ImportAsset(settingsPath);
                Debug.Log(builder.ToString());
            }

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

            var executorView = settingsElement.Q<VisualElement>("executor-element");
            foreach (var executor in ConfigurationExecutors)
            {
                try
                {
                    VisualElement element = null;
                    if (executor is OptionalExecutor oe)
                    {
                        element = new VisualElement { name = "extension-listview-item" };

                        var header = new VisualElement { name = "extension-listview-item-header" };
                        header.AddToClassList("thunderkit-field");
                        var label = new Label { name = "extension-label", bindingPath = nameof(Executor.extensionName) };
                        header.Add(label);
                        var toggle = new Toggle { name = "extension-enabled-toggle", bindingPath = nameof(OptionalExecutor.enabled) };
                        header.Add(toggle);

                        element.Add(header);
                    }

                    var child = executor.CreateUI();
                    if (child != null)
                    {
                        if (element == null)
                            element = new VisualElement { name = "extension-listview-item" };
                        element.Add(child);
                    }

                    if (element != null)
                    {
                        executorView.Add(element);
                        element.Bind(new SerializedObject(executor));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            //ConfigureListView(settingsElement.Q<ListView>("import-extensions-listview"));

            var browseButton = settingsElement.Q<Button>("browse-button");
            browseButton.clickable.clicked -= BrowserForGame;
            browseButton.clickable.clicked += BrowserForGame;

            var loadButton = settingsElement.Q<Button>("load-button");
            loadButton.clickable.clicked -= LoadGame;
            loadButton.clickable.clicked += LoadGame;

            if (thunderKitSettingsSO == null)
                thunderKitSettingsSO = new SerializedObject(this);

            rootElement.Bind(thunderKitSettingsSO);
        }

        private void ConfigureListView(ListView importProcessorsListView)
        {
            importProcessorsListView.bindItem = (visualElement, index) =>
            {
                var label = visualElement.Q<Label>("extension-label");
                var toggle = visualElement.Q<Toggle>("extension-enabled-toggle");

                var instance = importProcessorsListView.itemsSource[index];
                var type = instance.GetType();
                var enabledProperty = type.GetProperty("Enabled", publicInstanceBinding);
                var nameProperty = type.GetProperty("Name", publicInstanceBinding);

                label.text = nameProperty.GetValue(instance) as string;

                var onEnabledChanged = new EventCallback<ChangeEvent<bool>>(evt =>
                {
                    var enabled = (bool)evt.newValue;
                    enabledProperty.SetValue(importProcessorsListView.itemsSource[index], enabled);
                });
                toggle.value = (bool)enabledProperty.GetValue(importProcessorsListView.itemsSource[index]);
#if UNITY_2019_1_OR_NEWER
                if (toggle.userData is EventCallback<ChangeEvent<bool>> callback)
                    toggle.UnregisterValueChangedCallback(callback);

                toggle.RegisterValueChangedCallback(onEnabledChanged);
#elif UNITY_2018_1_OR_NEWER
                if (toggle.userData is EventCallback<ChangeEvent<bool>> callback)
                    toggle.UnregisterCallback(callback);

                toggle.OnValueChanged(onEnabledChanged);
#endif
                toggle.userData = onEnabledChanged;
            };

            importProcessorsListView.itemsSource = ConfigurationExecutors.ToList();
        }

        private async void LoadGame()
        {
            await GameConfigurator.LoadGame(this);
        }
        private void BrowserForGame()
        {
            GameConfigurator.LocateGame(this);
            if (!string.IsNullOrEmpty(GameExecutable) && !string.IsNullOrEmpty(GamePath))
            {
                if (markdown != null)
                    markdown.RemoveFromHierarchy();
            }
        }

        public void SetQuickAccess(Pipeline pipeline, bool quickAccess)
        {
            if (quickAccess)
                QuickAccessPipelines = (QuickAccessPipelines ?? System.Array.Empty<Pipeline>()).Where(a => a).Append(pipeline).OrderBy(m => m.name).ToArray();
            else
                QuickAccessPipelines = (QuickAccessPipelines ?? System.Array.Empty<Pipeline>()).Where(a => a).Where(m => m.name != pipeline.name).OrderBy(name => name).ToArray();
        }
        public void SetQuickAccess(Manifest manifest, bool quickAccess)
        {
            if (quickAccess)
                QuickAccessManifests = (QuickAccessManifests ?? System.Array.Empty<Manifest>()).Where(a => a).Append(manifest).OrderBy(m => m.name).ToArray();
            else
                QuickAccessManifests = (QuickAccessManifests ?? System.Array.Empty<Manifest>()).Where(a => a).Where(m => m.name != manifest.name).OrderBy(name => name).ToArray();
        }
    }
}
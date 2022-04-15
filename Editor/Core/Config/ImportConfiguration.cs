#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ThunderKit.Core.Config
{
    using static AssetDatabase;
    using static ThunderKit.Common.PathExtensions;

    public class ImportConfiguration : ThunderKitSetting
    {
        public Executor[] ConfigurationExecutors;
        public int ConfigurationIndex = -1;

        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.update += SetupConfigurators;
        }

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            string templatePath = Path.Combine(Constants.SettingsTemplatesPath, $"{nameof(ImportConfiguration)}.uxml");
            var settingsElement = TemplateHelpers.LoadTemplateInstance(templatePath);
            settingsElement.AddEnvironmentAwareSheets(templatePath);
            rootElement.Add(settingsElement);

            var executorView = settingsElement.Q<VisualElement>("executor-element");
            foreach (var executor in ConfigurationExecutors)
            {
                VisualElement element = null;
                try
                {
                    var child = executor.CreateUI();
                    if (child != null)
                    {
                        if (element == null)
                            element = new VisualElement { name = "extension-listview-item" };

                        element.Add(child);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    element = new VisualElement { name = "extension-item" };

                    var header = new VisualElement { name = "extension-item-header" };
                    header.AddToClassList("thunderkit-field");
                    var label = new Label { name = "extension-label", text = executor.Name };
                    header.Add(label);
                    var errorLabel = new Label { name = "extension-error-label", text = "Error", tooltip = e.Message };
                    header.Add(errorLabel);

                    element.Add(header);
                }

                if (element != null)
                {
                    executorView.Add(element);
                    element.Bind(new SerializedObject(executor));
                }
            }
        }

        private static IEnumerable<Assembly> configurationAssemblies;

        private static void SetupConfigurators()
        {
            if (EditorApplication.isUpdating) return;
            var settings = GetOrCreateSettings<ImportConfiguration>();

            var builder = new StringBuilder("Loaded GameConfigurators:");
            builder.AppendLine();
            Type[] loadedTypes = null;
            if (configurationAssemblies == null)
            {
                configurationAssemblies = AppDomain.CurrentDomain
                                .GetAssemblies()
                                .Where(asm => asm != null)
                                .Where(asm => asm.GetCustomAttribute<ImportExtensionsAttribute>() != null);

                loadedTypes = configurationAssemblies
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


                var settingsPath = GetAssetPath(settings);
                var objects = LoadAllAssetRepresentationsAtPath(settingsPath);
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

                for (int i = 0; i < executors.Count; i++)
                {
                    var executor = executors[i];
                    if (GetAssetPath(executor) != settingsPath)
                        AddObjectToAsset(executor, settingsPath);
                }

                var updatedExectors = executors.ToArray();
                settings.ConfigurationExecutors = updatedExectors;
                if (currentExecutorCount != updatedExectors.Length)
                {
                    ImportAsset(settingsPath);
                    Debug.Log(builder.ToString());
                }
            }

            settings.ImportGame();
        }

        public void ImportGame()
        {
            var thunderKitSettings = GetOrCreateSettings<ThunderKitSettings>();
            if (ConfigurationIndex >= ConfigurationExecutors.Length || ConfigurationIndex < 0) return;

            if (string.IsNullOrEmpty(thunderKitSettings.GamePath) || string.IsNullOrEmpty(thunderKitSettings.GameExecutable))
            {
                LocateGame(thunderKitSettings);
                return;
            }

            if (!CheckUnityVersion(thunderKitSettings)) return;

            var executor = ConfigurationExecutors[ConfigurationIndex];
            try
            {
                switch (executor)
                {
                    case OptionalExecutor optional:
                        if (optional.enabled)
                            optional.Execute();
                        break;
                    default:
                        executor.Execute();
                        break;
                }
            }
            catch (Exception e)
            {
                ConfigurationIndex = ConfigurationExecutors.Length + 1;
                Debug.LogError(e);
                return;
            }
            ConfigurationIndex++;

            if (ConfigurationIndex >= ConfigurationExecutors.Length)
                ImportAsset(thunderKitSettings.PackageFilePath);
        }

        public static void LocateGame(ThunderKitSettings tkSettings)
        {
            string currentDir = Directory.GetCurrentDirectory();
            var foundExecutable = false;

            while (!foundExecutable)
            {
                var path = string.Empty;
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "exe");
                        break;
                    case RuntimePlatform.LinuxEditor:
                        path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "*");
                        break;
                    //case RuntimePlatform.OSXEditor:
                    //    path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "app");
                    //    break;
                    default:
                        EditorUtility.DisplayDialog("Unsupported", "Your operating system is partially or completely unsupported. Contributions to improve this are welcome", "Ok");
                        return;
                }
                if (string.IsNullOrEmpty(path)) return;
                //For Linux, we will have to check the selected file to see if the GameExecutable_Data folder exists, we can use this to verify the executable was selected
                tkSettings.GameExecutable = Path.GetFileName(path);
                tkSettings.GamePath = Path.GetDirectoryName(path);
                foundExecutable = Directory.GetFiles(tkSettings.GamePath, tkSettings.GameExecutable).Any();
            }
            EditorUtility.SetDirty(tkSettings);
        }

        private static bool CheckUnityVersion(ThunderKitSettings settings)
        {
            var versionMatch = false;
            var regs = new Regex(".*?(\\d{1,4}\\.\\d+\\.\\d+\\w\\d+).*");

            var unityVersion = regs.Replace(Application.unityVersion, match => match.Groups[1].Value);

            var informationFile = Combine(settings.GameDataPath, "globalgamemanagers");
            var playerVersion = string.Empty;
            if (!File.Exists(informationFile))
            {
                informationFile = Combine(settings.GameDataPath, "data.unity3d");
            }
            if (File.Exists(informationFile))
            {
                var am = new AssetsManager();
                var ggm = am.LoadAssetsFile(informationFile, false);

                playerVersion = ggm.table.file.typeTree.unityVersion;

                am.UnloadAll(true);

                versionMatch = unityVersion.Equals(playerVersion);
            }
            else
            {
                var exePath = Combine(settings.GamePath, settings.GameExecutable);
                var fvi = FileVersionInfo.GetVersionInfo(exePath);
                playerVersion = fvi.FileVersion.Substring(0, fvi.FileVersion.LastIndexOf("."));
                if (playerVersion.Count(f => f == '.') == 2)
                    versionMatch = unityVersion.Equals(playerVersion);
            }

            if (!versionMatch)
                Debug.Log($"Unity Editor version ({unityVersion}), Unity Player version ({playerVersion}), aborting setup." +
                      $"\r\n\t Make sure you're using the same version of the Unity Editor as the Unity Player for the game.");
            return versionMatch;
        }

    }
}

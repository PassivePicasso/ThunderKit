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
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using ThunderKit.Core.Config;

namespace ThunderKit.Core.Data
{
    using static AssetDatabase;
    using static ThunderKit.Common.PathExtensions;

    public class ImportConfiguration : ThunderKitSetting
    {
        public OptionalExecutor[] ConfigurationExecutors;
        public int ConfigurationIndex = -1;

        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.update += StepImporters;

            var settings = GetOrCreateSettings<ImportConfiguration>();
            settings.Initialize();
        }


        private static void StepImporters()
        {
            if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
            {
                var settings = GetOrCreateSettings<ImportConfiguration>();
                var settingsPath = GetAssetPath(settings);
                var executors = LoadAllAssetRepresentationsAtPath(settingsPath)
                                .OfType<OptionalExecutor>()
                                .OrderByDescending(executor => executor.Priority)
                                .ToArray();

                if (settings.ConfigurationExecutors == null || !executors.SequenceEqual(settings.ConfigurationExecutors))
                    settings.ConfigurationExecutors = executors;

                settings.ImportGame();
            }
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
                if (!executor) continue;
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

        public override void Initialize()
        {
            Type[] loadedTypes = null;
            var configurationAssemblies = AppDomain.CurrentDomain
                            .GetAssemblies()
                            .Where(asm => asm != null)
                            .Where(asm => asm.GetCustomAttribute<ImportExtensionsAttribute>() != null);
            loadedTypes = configurationAssemblies
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

            var builder = new StringBuilder("Loaded Import Extensions");
            builder.AppendLine();
            var settingsPath = GetAssetPath(this);

            var executorTypes = loadedTypes.Where(t => typeof(OptionalExecutor).IsAssignableFrom(t)).ToArray();
            var objs = LoadAllAssetRepresentationsAtPath(settingsPath).Where(obj => obj).ToArray();
            var distinctObjcs = objs.Distinct().ToArray();
            var objectTypes = distinctObjcs.Select(obj => obj.GetType()).ToArray();
            var existingAssetTypes = new HashSet<Type>(objectTypes);
            foreach (var t in executorTypes)
            {
                if (existingAssetTypes.Contains(t))
                    continue;

                var executor = CreateInstance(t) as OptionalExecutor;
                if (executor)
                {
                    AddObjectToAsset(executor, this);
                    executor.name = executor.Name;
                    builder.AppendLine(executor.GetType().FullName);
                }
            }
            EditorApplication.update += DoImport;
        }


        private void DoImport()
        {
            var settingsPath = GetAssetPath(this);
            ImportAsset(settingsPath, ImportAssetOptions.ForceUpdate);
            EditorApplication.update -= DoImport;
        }

        public void ImportGame()
        {
            var thunderKitSettings = GetOrCreateSettings<ThunderKitSettings>();
            if (ConfigurationIndex >= (ConfigurationExecutors?.Length ?? 0) || ConfigurationIndex < 0) return;

            if (string.IsNullOrEmpty(thunderKitSettings.GamePath) || string.IsNullOrEmpty(thunderKitSettings.GameExecutable))
            {
                LocateGame(thunderKitSettings);
                return;
            }

            if (!CheckUnityVersion(thunderKitSettings)) return;

            var executor = ConfigurationExecutors[ConfigurationIndex];
            try
            {
                if (executor.enabled) executor.Execute();
            }
            catch (Exception e)
            {
                ConfigurationIndex = ConfigurationExecutors.Length + 1;
                Debug.LogError(e);
                return;
            }
            ConfigurationIndex++;
            AssetDatabase.Refresh();
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

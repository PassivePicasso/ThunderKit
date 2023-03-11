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
        public OptionalExecutor[] ConfigurationExecutors = Array.Empty<OptionalExecutor>();
        public int ConfigurationIndex = -1;
        [SerializeField, HideInInspector] private int totalImportExtensionCount = -1;

        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.update += StepImporters;
            ImportConfiguration configInstance = GetOrCreateSettings<ImportConfiguration>();
            string assetPath = GetAssetPath(configInstance);
            string guid = AssetPathToGUID(assetPath);
            if (configInstance.CheckForNewImportConfigs(out var executors))
            {
                var objs = LoadAllAssetRepresentationsAtPath(assetPath).ToArray();
                configInstance.ConfigurationExecutors = Array.Empty<OptionalExecutor>();
                foreach (var obj in objs)
                {
                    if (obj)
                    {
                        RemoveObjectFromAsset(obj);
                        DestroyImmediate(obj, true);
                    }
                }
                ComposableObject.FixMissingScriptSubAssets(configInstance);
                configInstance = LoadAssetAtPath<ImportConfiguration>(GUIDToAssetPath(guid));
                configInstance.LoadImportExtensions(executors);
                SaveAssets();
            }
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
            var configAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            FilterForConfigurationAssemblies(configAssemblies);
            var executorTypes = GetOptionalExecutors(configAssemblies);
            totalImportExtensionCount = executorTypes.Count;
            LoadImportExtensions(executorTypes);
            SaveAssets();
            EditorApplication.update += DoImport;
        }

        private bool CheckForNewImportConfigs(out List<Type> executorTypes)
        {
            List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            FilterForConfigurationAssemblies(assemblies);
            executorTypes = GetOptionalExecutors(assemblies);
            if (executorTypes.Count == totalImportExtensionCount)
            {
                executorTypes = null;
                return false;
            }
            totalImportExtensionCount = executorTypes.Count;
            return true;
        }

        private void FilterForConfigurationAssemblies(List<Assembly> assemblies)
        {
            for (int i = assemblies.Count - 1; i >= 0; i--)
            {
                try
                {
                    var asm = assemblies[i];
                    try
                    {
                        if (asm?.GetCustomAttribute<ImportExtensionsAttribute>() == null)
                            assemblies.RemoveAt(i);
                    }
                    catch
                    {
                        Debug.LogError($"Failed to analyze {asm.Location} for ImportExtensions");
                        assemblies.RemoveAt(i);
                    }
                }
                catch (Exception ex) { Debug.LogError(ex.Message); }
            }
        }

        private List<Type> GetOptionalExecutors(List<Assembly> assemblies)
        {
            return assemblies
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
               .Where(t => typeof(OptionalExecutor).IsAssignableFrom(t))
               .ToList();
        }

        private void LoadImportExtensions(List<Type> executorTypes)
        {
            var settingsPath = GetAssetPath(this);
            var builder = new StringBuilder("Loaded Import Extensions");
            builder.AppendLine();
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
        }

        private void DoImport()
        {
            var settingsPath = GetAssetPath(this);
            if (string.IsNullOrEmpty(settingsPath)) return;
            ImportAsset(settingsPath, ImportAssetOptions.ForceUpdate);
            EditorApplication.update -= DoImport;
        }

        public void ImportGame()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (ConfigurationExecutors == null) return;

            var thunderKitSettings = GetOrCreateSettings<ThunderKitSettings>();

            if (ConfigurationIndex >= (ConfigurationExecutors?.Length ?? 0) || ConfigurationIndex < 0) return;

            if (string.IsNullOrEmpty(thunderKitSettings.GamePath) || string.IsNullOrEmpty(thunderKitSettings.GameExecutable))
            {
                if (!LocateGame(thunderKitSettings))
                {
                    ConfigurationIndex = -1;
                }
                return;
            }

            if (!CheckUnityVersion(thunderKitSettings))
            {
                ConfigurationIndex = -1;
                return;
            }

            var executor = ConfigurationExecutors[ConfigurationIndex];
            try
            {
                if (executor && executor.enabled)
                {
                    if (executor.Execute())
                    {
                        Debug.Log($"Executed: {executor.name}");
                        ConfigurationIndex++;
                        AssetDatabase.Refresh();
                    }
                }
                else
                    ConfigurationIndex++;
            }
            catch (Exception e)
            {
                ConfigurationIndex = ConfigurationExecutors.Length + 1;
                Debug.LogError($"Error during Import: {e}");
                return;
            }
            if (ConfigurationIndex >= ConfigurationExecutors.Length)
            {
                foreach (var ce in ConfigurationExecutors)
                    ce.Cleanup();
            }
        }

        public static bool LocateGame(ThunderKitSettings tkSettings)
        {
            var foundExecutable = false;

            while (!foundExecutable)
            {
                var path = string.IsNullOrEmpty(tkSettings.GamePath) ? Directory.GetCurrentDirectory() : tkSettings.GamePath;
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        path = EditorUtility.OpenFilePanel("Open Game Executable", path, "exe");
                        break;
                    case RuntimePlatform.LinuxEditor:
                        path = EditorUtility.OpenFilePanel("Open Game Executable", path, "");
                        break;
                    //case RuntimePlatform.OSXEditor:
                    //    path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "app");
                    //    break;
                    default:
                        EditorUtility.DisplayDialog("Unsupported", "Your operating system is partially or completely unsupported. Contributions to improve this are welcome", "Ok");
                        return false;
                }
                if (string.IsNullOrEmpty(path)) return false;
                //For Linux, we will have to check the selected file to see if the GameExecutable_Data folder exists, we can use this to verify the executable was selected
                tkSettings.GameExecutable = Path.GetFileName(path);
                tkSettings.GamePath = Path.GetDirectoryName(path);
                foundExecutable = Directory.GetFiles(tkSettings.GamePath, tkSettings.GameExecutable).Any();
            }
            EditorUtility.SetDirty(tkSettings);
            return true;
        }


        private static bool CheckUnityVersion(ThunderKitSettings settings)
        {
            var versionMatch = false;
            var regs = new Regex(".*?(\\d{1,4}\\.\\d+\\.\\d+\\w\\d+).*");

            var unityVersion = regs.Replace(Application.unityVersion, match => match.Groups[1].Value);
            var playerVersion = string.Empty;

            var informationFile = Combine(settings.GameDataPath, "globalgamemanagers");
            if (!File.Exists(informationFile))
                informationFile = Combine(settings.GameDataPath, "data.unity3d");

            bool foundVersion = false;
            if (File.Exists(informationFile))
            {
                try
                {
                    var am = new AssetsManager();
                    var ggm = am.LoadAssetsFile(informationFile, false);

                    playerVersion = ggm.table.file.typeTree.unityVersion;

                    am.UnloadAll(true);

                    versionMatch = unityVersion.Equals(playerVersion);
                    foundVersion = true;
                }
                catch (Exception ex)
                {
                    foundVersion = false;
                }
            }

            if (!foundVersion)
            {
                var exePath = Combine(settings.GamePath, settings.GameExecutable);
                var fvi = FileVersionInfo.GetVersionInfo(exePath);
                playerVersion = fvi.FileVersion.Substring(0, fvi.FileVersion.LastIndexOf("."));
                if (playerVersion.Count(f => f == '.') == 2)
                    versionMatch = unityVersion.Equals(playerVersion);
            }

            if (!versionMatch)
                Debug.LogError($"Unity Editor version ({unityVersion}), Unity Player version ({playerVersion}), aborting setup." +
                      $"\r\n\t Make sure you're using the same version of the Unity Editor as the Unity Player for the game.");
            return versionMatch;
        }


    }
}

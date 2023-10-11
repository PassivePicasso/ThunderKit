#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThunderKit.Common;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEngine;
using ThunderKit.Core.Config;
using UnityEngine.Serialization;
using System.Reflection;
using ThunderKit.Core.Utilities;
using System.CodeDom.Compiler;

namespace ThunderKit.Core.Data
{
    using static AssetDatabase;

    public class ImportConfiguration : ThunderKitSetting
    {
        public OptionalExecutor[] ConfigurationExecutors = Array.Empty<OptionalExecutor>();
        [SerializeField]
        [FormerlySerializedAs("ConfigurationIndex")]
        private int configurationIndex = -1;
        public int ConfigurationIndex
        {
            get => configurationIndex;
            set
            {
                if (value == configurationIndex)
                {
                    return;
                }
                configurationIndex = value;
                EditorUtility.SetDirty(this);
            }
        }
        [SerializeField, HideInInspector] private int totalImportExtensionCount = -1;

        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.update += StepImporters;
        }

        private static void StepImporters()
        {
            var configInstance = GetOrCreateSettings<ImportConfiguration>();
            if (configInstance.CheckForNewImportConfigs(out _))
            {
                configInstance = RecreateSelf(configInstance);
                SaveAssets();
            }

            configInstance.ImportGame();
        }

        static ImportConfiguration RecreateSelf(ImportConfiguration configInstance)
        {
            var clone = CreateInstance<ImportConfiguration>();

            var configInstancePath = GetAssetPath(configInstance);
            var clonePath = configInstancePath.Replace(".asset", "CLONE.asset");
            CreateAsset(clone, clonePath);
            ImportAsset(clonePath);
            clone.Initialize();
            ImportAsset(clonePath);

            foreach (var executor in configInstance.ConfigurationExecutors)
            {
                if (!executor) continue;
                var friend = clone.ConfigurationExecutors.FirstOrDefault(oe => oe.GetType() == executor.GetType());
                if (friend)
                    EditorUtility.CopySerialized(executor, friend);
            }

            DeleteAsset(configInstancePath);
            AssetDatabase.Refresh();
            clone.name = nameof(ImportConfiguration);
            var message = RenameAsset(clonePath, nameof(ImportConfiguration));
            if (!string.IsNullOrEmpty(message))
                Debug.LogError(message);

            AssetDatabase.Refresh();
            return clone;
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
            if (CheckForNewImportConfigs(out var executorTypes))
            {
                LoadImportExtensions(executorTypes);
                SaveAssets();
            }
        }

        private bool CheckForNewImportConfigs(out List<Type> executorTypes)
        {
            executorTypes = GetOptionalExecutors();
            if (executorTypes.Count == totalImportExtensionCount)
            {
                executorTypes = null;
                return false;
            }
            totalImportExtensionCount = executorTypes.Count;
            return true;
        }

        private List<Type> GetOptionalExecutors()
        {
#if UNITY_2019_2_OR_NEWER
            return TypeCache.GetTypesDerivedFrom<OptionalExecutor>()
                .Where(t => t.Assembly.GetCustomAttribute<ImportExtensionsAttribute>() != null)
#else
            return FilterForConfigurationAssemblies()
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
               .Where(t => typeof(OptionalExecutor).IsAssignableFrom(t))
#endif
               .Where(t => t != null)
                .Where(t => !t.IsAbstract && !t.IsInterface)
               .ToList();
        }
#if UNITY_2019_2_OR_NEWER
#else
        private List<Assembly> FilterForConfigurationAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
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
            return assemblies;
        }
#endif

        private void LoadImportExtensions(List<Type> executorTypes, Dictionary<Type, bool> previousStates = null)
        {
            var settingsPath = GetAssetPath(this);
            var builder = new StringBuilder("Loaded Import Extensions");
            builder.AppendLine();
            var existingAssetTypes = new HashSet<Type>(ConfigurationExecutors.Select(obj => obj.GetType()));
            foreach (var t in executorTypes)
            {
                if (existingAssetTypes.Contains(t))
                    continue;

                var executor = CreateInstance(t) as OptionalExecutor;
                if (executor)
                {
                    AddObjectToAsset(executor, this);
                    executor.name = executor.Name;
                    if (previousStates != null && previousStates.TryGetValue(t, out var enabled))
                    {
                        executor.enabled = enabled;
                    }
                    builder.AppendLine(executor.GetType().FullName);
                }
            }
            EditorUtility.SetDirty(this);
            SaveAssets();
            ImportAsset(settingsPath);
            var assetReps = LoadAllAssetRepresentationsAtPath(settingsPath);
            var optionalExecutors = assetReps.OfType<OptionalExecutor>().ToArray();
            var orderedExecutors = optionalExecutors.OrderByDescending(executor => executor.Priority).ToArray();
            ConfigurationExecutors = orderedExecutors.ToArray();
        }

        public void ImportGame()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (ConfigurationExecutors == null) return;

            var thunderKitSettings = GetOrCreateSettings<ThunderKitSettings>();

            if (ConfigurationIndex >= (ConfigurationExecutors?.Length ?? 0) || ConfigurationIndex < 0) return;

            try
            {
                if (string.IsNullOrEmpty(thunderKitSettings.GamePath) || string.IsNullOrEmpty(thunderKitSettings.GameExecutable))
                {
                    if (!LocateGame(thunderKitSettings))
                    {
                        ConfigurationIndex = -1;
                    }
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
                        }
                    }
                    else
                        ConfigurationIndex++;
                }
                catch (Exception e)
                {
                    ConfigurationIndex = -1;
                    throw new Exception("Import Failed", e);
                }
            }
            finally
            {
                if (ConfigurationIndex >= ConfigurationExecutors.Length || ConfigurationIndex < 0)
                    Cleanup();
            }
        }

        private void Cleanup()
        {
            foreach (var ce in ConfigurationExecutors)
            {
                try
                {
                    if (ce.enabled)
                        ce.Cleanup();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error during Cleanup: {e}");
                }
            }
            Refresh();
        }

        private static void Refresh()
        {
            Debug.Log($"Executing Refresh...\r\nUnlock Assemblies, Refresh Asset Database, Resolve Packages");
            AssetDatabase.SaveAssets();
            PackageHelper.ResolvePackages();
            AssetDatabase.Refresh();
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


    }
}

using AssetsExporter;
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
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace ThunderKit.Core.Config
{
    using static ThunderKit.Common.PathExtensions;

    public static class ConfigureGame
    {
        private static readonly HashSet<string> EmptySet = new HashSet<string>();
        public static IReadOnlyList<BlacklistProcessor> BlacklistProcessors { get; private set; }
        public static IReadOnlyList<WhitelistProcessor> WhitelistProcessors { get; private set; }
        public static IReadOnlyList<AssemblyProcessor> AssemblyProcessors { get; private set; }
        public static IReadOnlyList<ConfigureAction> ConfigureActions { get; private set; }

        [InitializeOnLoadMethod]
        static void InitializeConfigurators()
        {
            var builder = new StringBuilder("Loaded GameConfigurators:");
            builder.AppendLine();
            var configurationAssemblies = AppDomain.CurrentDomain
                            .GetAssemblies()
                            .Where(asm => asm != null)
                            .Where(asm => asm.GetCustomAttribute<GameConfiguratorAssemblyAttribute>() != null);
            var loadedTypes = configurationAssemblies
               .SelectMany(asm =>
               {
                   try
                   {
                       return asm.GetTypes();
                   }
                   catch (ReflectionTypeLoadException e)
                   {
                       return e.Types.Where(t => t != null);
                   }
               }).ToArray();

            BlacklistProcessors = CreateImporters<BlacklistProcessor>(builder, loadedTypes);
            WhitelistProcessors = CreateImporters<WhitelistProcessor>(builder, loadedTypes);
            AssemblyProcessors = CreateImporters<AssemblyProcessor>(builder, loadedTypes);
            ConfigureActions = CreateImporters<ConfigureAction>(builder, loadedTypes);

            Debug.Log(builder.ToString());
        }

        private static List<T> CreateImporters<T>(StringBuilder builder, Type[] loadedTypes) where T : ImportExtension<T>
        {
            var importers = loadedTypes.Where(t => typeof(T).IsAssignableFrom(t))
                .Select(t =>
                {
                    var configurator = Activator.CreateInstance(t) as T;
                    builder.AppendLine(configurator.GetType().AssemblyQualifiedName);
                    return configurator;
                })
                .Where(t => t != null)
                .ToList();
            importers.Sort();
            return importers;
        }

        public static void LoadGame(ThunderKitSettings settings)
        {
            if (string.IsNullOrEmpty(settings.GamePath) || string.IsNullOrEmpty(settings.GameExecutable))
            {
                LocateGame(settings);
                return;
            }

            SetBitness(settings);
            EditorUtility.SetDirty(settings);

            if (!CheckUnityVersion(settings)) return;

            var packageName = Path.GetFileNameWithoutExtension(settings.GameExecutable);
            AssertDestinations(packageName);

            ImportAssemblies(packageName, settings);

            ImportGameSettings(settings);

            foreach (var configurator in ConfigureActions)
                if (configurator.Enabled)
                    configurator.Execute();

            EditorApplication.update += UpdateGamePackage;
            try
            {
                AssetDatabase.Refresh();
            }
            catch
            {
                Debug.LogWarning("Error during refresh");
            }
        }

        private static void UpdateGamePackage()
        {
            if (EditorApplication.isUpdating) return;
            EditorApplication.update -= UpdateGamePackage;
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var packageName = Path.GetFileNameWithoutExtension(settings.GameExecutable);
            SetupPackageManifest(settings, packageName);
        }

        private static void ImportGameSettings(ThunderKitSettings settings)
        {
            var classDataPath = Path.GetFullPath(Combine(Constants.ThunderKitRoot, "Editor", "ThirdParty", "AssetsTools.NET", "classdata.tpk"));

            var unityVersion = Application.unityVersion;
            var gameManagerTemp = Combine(Directory.GetCurrentDirectory(), "Temp", "ImportedProjectSettings");
            var editorDirectory = Path.GetDirectoryName(EditorApplication.applicationPath);
            var executablePath = Combine(settings.GamePath, settings.GameExecutable);
            GameExporter.ExportGlobalGameManagers(executablePath, gameManagerTemp, settings.GameDataPath, editorDirectory, classDataPath, unityVersion);

            var includedSettings = (IncludedSettings)settings.IncludedSettings;
            foreach (IncludedSettings include in (IncludedSettings[])Enum.GetValues(typeof(IncludedSettings)))
            {
                if (!includedSettings.HasFlag(include)) continue;

                string settingName = $"{include}.asset";
                string settingPath = Combine("ProjectSettings", settingName);
                string tempSettingPath = Combine(gameManagerTemp, "ProjectSettings", settingName);
                if (!File.Exists(tempSettingPath)) continue;

                File.Copy(tempSettingPath, settingPath, true);
                //Update times as necessary to trigger unity import
                File.SetLastWriteTime(settingPath, DateTime.Now);
                File.SetLastAccessTime(settingPath, DateTime.Now);
                File.SetCreationTime(settingPath, DateTime.Now);
            }
        }

        private static void SetupPackageManifest(ThunderKitSettings settings, string packageName)
        {
            PackageHelper.GeneratePackageManifest(settings.PackageName, settings.PackageFilePath, packageName, PlayerSettings.companyName, Application.version, $"Imported assemblies from game {packageName}");
        }

        private static void AssertDestinations(string packageName)
        {
            var destinationFolder = Combine("Packages", packageName);
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            destinationFolder = Combine("Packages", packageName, "plugins");
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);
        }

        public static void LocateGame(ThunderKitSettings settings)
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
                settings.GameExecutable = Path.GetFileName(path);
                settings.GamePath = Path.GetDirectoryName(path);
                foundExecutable = Directory.GetFiles(settings.GamePath, settings.GameExecutable).Any();
            }
            EditorUtility.SetDirty(settings);
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

            var result = versionMatch ? "" : ", aborting setup.\r\n\t Make sure you're using the same version of the Unity Editor as the Unity Player for the game.";
            Debug.Log($"Unity Editor version ({unityVersion}), Unity Player version ({playerVersion}){result}");
            return versionMatch;
        }

        private static void ImportAssemblies(string packageName, ThunderKitSettings settings)
        {
            try
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

                AssetDatabase.StartAssetEditing();
                EditorApplication.LockReloadAssemblies();

                var blackList = BuildAssemblyBlacklist();
                var whitelist = BuildAssemblyWhitelist(settings, nativeAssemblyExtension);

                var packagePath = Combine("Packages", packageName);
                var managedAssemblies = Directory.EnumerateFiles(settings.ManagedAssembliesPath, "*.dll", SearchOption.AllDirectories).Distinct().ToList();

                ImportFilteredAssemblies(packagePath, managedAssemblies, blackList, whitelist);

                var pluginsPath = Combine(settings.GameDataPath, "Plugins");
                if (Directory.Exists(pluginsPath))
                {
                    var packagePluginsPath = Combine(packagePath, "plugins");
                    var plugins = Directory.EnumerateFiles(pluginsPath, $"*.{nativeAssemblyExtension}", SearchOption.AllDirectories);
                    ImportFilteredAssemblies(packagePluginsPath, plugins, EmptySet, EmptySet);
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
                AssetDatabase.StopAssetEditing();
            }
        }

        private static HashSet<string> BuildAssemblyWhitelist(ThunderKitSettings settings, string nativeAssemblyExtension)
        {
            string[] installedGameAssemblies = Array.Empty<string>();
            if (Directory.Exists(settings.PackagePath))
                installedGameAssemblies = Directory.EnumerateFiles(settings.PackagePath, $"*.dll", SearchOption.AllDirectories)
                                       .Union(Directory.EnumerateFiles(settings.PackagePath, $"*.{nativeAssemblyExtension}", SearchOption.AllDirectories))
                                       .Select(path => Path.GetFileName(path))
                                       .Distinct()
                                       .ToArray();


            var whitelist = new HashSet<string>(installedGameAssemblies);

            var enumerable = whitelist as IEnumerable<string>;

            foreach (var processor in WhitelistProcessors)
                if (processor.Enabled)
                    enumerable = processor.Process(enumerable);
            return whitelist;
        }
        /// <summary>
        /// Collect list of Assemblies that should not be imported from the game.
        /// These are assemblies that would be automatically provided by Unity to the environment
        /// </summary>
        /// <param name="byEditorFiles"></param>
        private static HashSet<string> BuildAssemblyBlacklist(bool byEditorFiles = false)
        {
            var result = new HashSet<string>();
            if (byEditorFiles)
            {
                var editorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
                var extensionsFolder = Combine(editorPath, "Data", "Managed");
                foreach (var asmFile in Directory.GetFiles(extensionsFolder, "*.dll", SearchOption.AllDirectories))
                {
                    result.Add(Path.GetFileName(asmFile));
                }
            }
            else
            {
                var blackList = AppDomain.CurrentDomain.GetAssemblies()
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
                .Select(asm => asm.Location)
                    .Select(location =>
                    {
                        try
                        {
                            return Path.GetFileName(location);
                        }
                        catch
                        {
                            return string.Empty;
                        }
                    })
                    .OrderBy(s => s);
                foreach (var asm in blackList)
                    result.Add(asm);
            }

            var enumerable = result as IEnumerable<string>;

            foreach (var processor in BlacklistProcessors)
                if (processor.Enabled)
                    enumerable = processor.Process(enumerable);

            return new HashSet<string>(enumerable);
        }

        private static void ImportFilteredAssemblies(string destinationFolder, IEnumerable<string> assemblies, HashSet<string> blackList, HashSet<string> whitelist)
        {
            foreach (var assemblyPath in assemblies)
            {
                var asmPath = assemblyPath.Replace("\\", "/");
                foreach (var processor in AssemblyProcessors)
                    if (processor.Enabled)
                        asmPath = processor.Process(asmPath);

                string assemblyFileName = Path.GetFileName(asmPath);
                if (!whitelist.Contains(assemblyFileName)
                  && blackList.Contains(assemblyFileName))
                    continue;

                var destinationFile = Combine(destinationFolder, assemblyFileName);

                var destinationMetaData = Combine(destinationFolder, $"{assemblyFileName}.meta");

                try
                {
                    if (File.Exists(destinationFile)) File.Delete(destinationFile);
                    File.Copy(asmPath, destinationFile);

                    PackageHelper.WriteAssemblyMetaData(asmPath, destinationMetaData);
                }
                catch
                {
                    Debug.LogWarning($"Could not update assembly: {destinationFile}", AssetDatabase.LoadAssetAtPath<Object>(destinationFile));
                }
            }
        }

        public static void SetBitness(ThunderKitSettings settings)
        {
            if (Application.platform != RuntimePlatform.WindowsEditor) return;
            var assembly = Combine(settings.GamePath, settings.GameExecutable);
            using (var stream = File.OpenRead(assembly))
            using (var binStream = new BinaryReader(stream))
            {
                stream.Seek(0x3C, SeekOrigin.Begin);
                if (binStream.PeekChar() != -1)
                {
                    var e_lfanew = binStream.ReadInt32();
                    stream.Seek(e_lfanew + 0x4, SeekOrigin.Begin);
                    var cpuType = binStream.ReadUInt16();
                    if (cpuType == 0x8664)
                    {
                        settings.Is64Bit = true;
                        return;
                    }
                }
            }
            settings.Is64Bit = false;
        }
    }
}

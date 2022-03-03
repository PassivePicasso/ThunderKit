using AssetsExporter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ThunderKit.Common;
using ThunderKit.Common.Package;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ThunderKit.Core.Config
{
    using static ThunderKit.Common.PathExtensions;
    public class ConfigureGame
    {
        private static readonly HashSet<string> EmptySet = new HashSet<string>();

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

            GetReferences(packageName, settings);

            SetupPackageManifest(settings, packageName);

            var classDataPath = Path.GetFullPath(Path.Combine(Constants.ThunderKitRoot, "Editor", "ThirdParty", "AssetsTools.NET", "classdata.tpk"));

            var unityVersion = Application.unityVersion;
            GameExporter.ExportGlobalGameManagers(Path.Combine(settings.GamePath, settings.GameExecutable), "e:\\TestOutput", Path.GetDirectoryName(EditorApplication.applicationPath), classDataPath, unityVersion);

            AssetDatabase.Refresh();
        }

        private static void SetupPackageManifest(ThunderKitSettings settings, string packageName)
        {
            var name = packageName.ToLower().Split(' ').Aggregate((a, b) => $"{a}{b}");
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(settings.GamePath, settings.GameExecutable));
            var outputDir = Combine("Packages", packageName);
            PackageHelper.GeneratePackageManifest(name, outputDir, packageName, fileVersionInfo.CompanyName, "1.0.0", $"Imported assemblies from game {packageName}");
        }

        private static void AssertDestinations(string packageName)
        {
            var destinationFolder = Path.Combine("Packages", packageName);
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
                    //case RuntimePlatform.OSXEditor:
                    //    path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "app");
                    //    break;
                    default:
                        EditorUtility.DisplayDialog("Unsupported", "Your operating system is partially or completely unsupported. Contributions to improve this are welcome", "Ok");
                        return;
                }
                if (string.IsNullOrEmpty(path)) return;
                settings.GameExecutable = Path.GetFileName(path);
                settings.GamePath = Path.GetDirectoryName(path);
                foundExecutable = Directory.GetFiles(settings.GamePath, settings.GameExecutable).Any();
            }
            EditorUtility.SetDirty(settings);

            //var ggms = Directory.EnumerateFiles(settings.GamePath, "globalgamemanagers", SearchOption.AllDirectories).ToArray();
            //if (ggms.Any())
            //{
            //    foreach (var ggm in ggms)
            //        Debug.Log($"Found GGM: {ggm}");
            //    //var am = new AssetsManager();
            //    //am.LoadBundleDependencies
            //}
        }

        private static bool CheckUnityVersion(ThunderKitSettings settings)
        {
            var versionMatch = false;
            var regs = new Regex(".*?(\\d{1,4}\\.\\d+\\.\\d+).*");

            var unityVersion = regs.Replace(Application.unityVersion, match => match.Groups[1].Value);

            var dataPath = Path.Combine(settings.GamePath, $"{Path.GetFileNameWithoutExtension(settings.GameExecutable)}_Data");
            var informationFile = Path.Combine(dataPath, "globalgamemanagers");
            var playerVersion = string.Empty;
            if (!File.Exists(informationFile))
            {
                informationFile = Path.Combine(dataPath, "data.unity3d");
            }
            if (File.Exists(informationFile))
            {
                var firstGrand = File.ReadLines(informationFile).First();

                playerVersion = regs.Replace(firstGrand, match => match.Groups[1].Value);

                versionMatch = unityVersion.Equals(playerVersion);
            }
            else
            {
                var exePath = Path.Combine(settings.GamePath, settings.GameExecutable);
                var fvi = FileVersionInfo.GetVersionInfo(exePath);
                playerVersion = fvi.FileVersion.Substring(0, fvi.FileVersion.LastIndexOf("."));
                if (playerVersion.Count(f => f == '.') == 2)
                    versionMatch = unityVersion.Equals(playerVersion);
            }

            var result = versionMatch ? "" : ", aborting setup.\r\n\t Make sure you're using the same version of the Unity Editor as the Unity Player for the game.";
            Debug.Log($"Unity Editor version ({unityVersion}), Unity Player version ({playerVersion}){result}");
            return versionMatch;
        }

        private static void GetReferences(string packageName, ThunderKitSettings settings)
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                EditorApplication.LockReloadAssemblies();
                Debug.Log("Acquiring references");
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

                var loadedGameAssemblies = Directory.EnumerateFiles(Path.Combine("Packages", packageName), "*.dll", SearchOption.AllDirectories).ToArray();

                var managedPath = Combine(settings.GamePath, $"{Path.GetFileNameWithoutExtension(settings.GameExecutable)}_Data", "Managed");
                var packagePath = Path.Combine("Packages", packageName);
                var managedAssemblies = Directory.GetFiles(managedPath, "*.dll");
                GetReferences(packagePath, managedAssemblies, new HashSet<string>(blackList.ToArray()), new HashSet<string>(loadedGameAssemblies));

                var pluginsPath = Combine(settings.GameDataPath, "Plugins");
                if (Directory.Exists(pluginsPath))
                {
                    var packagePluginsPath = Path.Combine(packagePath, "plugins");
                    var plugins = Directory.GetFiles(pluginsPath, "*.dll", SearchOption.AllDirectories);
                    GetReferences(packagePluginsPath, plugins, EmptySet, EmptySet);
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
                AssetDatabase.StopAssetEditing();
            }
        }

        private static void GetReferences(string destinationFolder, IEnumerable<string> assemblies, HashSet<string> blackList, HashSet<string> whitelist)
        {
            foreach (var assemblyPath in assemblies)
            {
                var asmPath = assemblyPath.Replace("\\", "/");
                string assemblyFileName = Path.GetFileName(asmPath);
                if (!whitelist.Contains(assemblyFileName)
                  && blackList.Contains(assemblyFileName))
                    continue;

                var destinationFile = Path.Combine(destinationFolder, assemblyFileName).Replace("\\", "/");

                var destinationMetaData = Path.Combine(destinationFolder, $"{assemblyFileName}.meta").Replace("\\", "/");

                if (File.Exists(destinationFile)) File.Delete(destinationFile);
                File.Copy(asmPath, destinationFile);

                PackageHelper.WriteAssemblyMetaData(asmPath, destinationMetaData);
            }
        }

        public static void SetBitness(ThunderKitSettings settings)
        {
            if (Application.platform != RuntimePlatform.WindowsEditor) return;
            var assembly = Path.Combine(settings.GamePath, settings.GameExecutable);
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

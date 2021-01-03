#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PassivePicasso.ThunderKit.Config
{
    public class ConfigureGame
    {
        [MenuItem(ScriptableHelper.ThunderKitMenuRoot + "Configure Game")]
        private static void Configure()
        {
            var settings = ThunderKitSettings.GetOrCreateSettings();

            LoadGame(settings);

            if (string.IsNullOrEmpty(settings.GamePath) || string.IsNullOrEmpty(settings.GameExecutable)) return;

            SetBitness(settings);
            EditorUtility.SetDirty(settings);

            if (!CheckUnityVersion(settings)) return;

            var packageName = Path.GetFileNameWithoutExtension(settings.GameExecutable);
            AssertDestinations(packageName);

            GetReferences(packageName, settings);
            EditorUtility.SetDirty(settings);

            SetupPackageManifest(settings, packageName);

            ScriptingSymbolManager.AddScriptingDefine("THUNDERKIT_CONFIGURED");

            AssetDatabase.ImportAsset("Packages", ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
        }

        private static void SetupPackageManifest(ThunderKitSettings settings, string packageName)
        {
            var name = packageName.ToLower().Split(' ').Aggregate((a, b) => $"{a}{b}");
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(settings.GamePath, settings.GameExecutable));
            var unityVersion = new Version(fileVersionInfo.FileVersion);
            var gameVersion = new Version(fileVersionInfo.FileVersion);
            var packageManifest = new PackageManagerManifest(name, packageName, "1.0.0", $"{unityVersion.Major}.{unityVersion.Minor}", $"Imported Assets from game {packageName}");
            var packageManifestJson = JsonUtility.ToJson(packageManifest);
            File.WriteAllText(Path.Combine("Packages", packageName, "package.json"), packageManifestJson);
        }

        private static void AssertDestinations(string packageName)
        {
            var destinationFolder = Path.Combine("Packages", packageName);
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            destinationFolder = Path.Combine("Packages", packageName, "plugins");
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);
        }

        private static void LoadGame(ThunderKitSettings settings)
        {
            string currentDir = Directory.GetCurrentDirectory();
            var foundExecutable = string.IsNullOrEmpty(settings.GamePath)
                                ? false
                                : Directory.EnumerateFiles(settings.GamePath ?? currentDir, Path.GetFileName(settings.GameExecutable)).Any();

            while (!foundExecutable)
            {
                string path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "exe");
                if (string.IsNullOrEmpty(path)) return;
                settings.GameExecutable = Path.GetFileName(path);
                settings.GamePath = Path.GetDirectoryName(path);
                foundExecutable = Directory.EnumerateFiles(settings.GamePath, settings.GameExecutable).Any();
            }
            EditorUtility.SetDirty(settings);
        }

        private static bool CheckUnityVersion(ThunderKitSettings settings)
        {
            var editorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
            var windowsStandalonePath = Path.Combine(editorPath, "Data", "PlaybackEngines", "windowsstandalonesupport");

            var regs = new Regex("(\\d\\d\\d\\d\\.\\d+\\.\\d+).*");

            var unityVersion = regs.Replace(Application.unityVersion, match => match.Groups[1].Value);

            var playerVersion = FileVersionInfo.GetVersionInfo(Path.Combine(settings.GamePath, settings.GameExecutable)).ProductVersion;
            playerVersion = regs.Replace(playerVersion, match => match.Groups[1].Value);

            var versionMatch = unityVersion.Equals(playerVersion);
            Debug.Log($"Unity Editor version ({unityVersion}), Unity Player version ({playerVersion}){(versionMatch ? "" : ", aborting setup.\r\n\t Make sure you're using the same version of the Unity Editor as the Unity Player for the game.")}");
            return versionMatch;
        }

        private static void GetReferences(string packageName, ThunderKitSettings settings)
        {
            Debug.Log("Acquiring references");
            var blackList = AppDomain.CurrentDomain.GetAssemblies().Where(asm => !asm.IsDynamic).Select(asm => asm.Location);
            if (settings?.excluded_assemblies != null)
                blackList = blackList.Union(settings?.excluded_assemblies).ToArray();

            var managedPath = Path.Combine(settings.GamePath, $"{Path.GetFileNameWithoutExtension(settings.GameExecutable)}_Data", "Managed");
            var pluginsPath = Path.Combine(settings.GamePath, $"{Path.GetFileNameWithoutExtension(settings.GameExecutable)}_Data", "Plugins");

            var managedAssemblies = Directory.EnumerateFiles(managedPath, "*.dll");
            var plugins = Directory.EnumerateFiles(pluginsPath, "*.dll");

            GetReferences(packageName, Path.Combine("Packages", packageName), managedAssemblies, settings.additional_assemblies, blackList, settings.assembly_metadata);
            GetReferences(packageName, Path.Combine("Packages", packageName, "plugins"), plugins, settings.additional_plugins, settings.excluded_assemblies, settings.assembly_metadata);
        }

        private static void GetReferences(string packageName, string destinationFolder, IEnumerable<string> assemblies, IEnumerable<string> whiteList, IEnumerable<string> blackList, IEnumerable<string> metaDataLocations)
        {
            if (whiteList == null) whiteList = Enumerable.Empty<string>();
            if (assemblies == null) assemblies = Enumerable.Empty<string>();
            if (blackList == null) blackList = Enumerable.Empty<string>();
            if (metaDataLocations == null) metaDataLocations = Enumerable.Empty<string>();

            var metaDataFiles = metaDataLocations.SelectMany(location =>
            {
                IEnumerable<string> enumerable = Directory.EnumerateFiles(location, "*.meta", SearchOption.TopDirectoryOnly);
                return enumerable;
            }).ToArray();

            foreach (var assembly in assemblies)
            {
                var filenameWithoutExtension = Path.GetFileNameWithoutExtension(assembly);
                Func<string, bool> matchingAssembly = enumerableAsm => enumerableAsm.Contains(filenameWithoutExtension);
                if (!whiteList.Any(matchingAssembly) && blackList.Any(matchingAssembly)) continue;

                var destinationFile = Path.Combine(destinationFolder, Path.GetFileName(assembly));

                var destinationMetaData = Path.Combine(destinationFolder, $"{Path.GetFileName(assembly)}.meta");

                if (File.Exists(destinationFile)) File.Delete(destinationFile);
                File.Copy(assembly, destinationFile);

                var metaData = metaDataFiles.FirstOrDefault(md => md.Contains(filenameWithoutExtension));
                if (!string.IsNullOrEmpty(metaData))
                    File.WriteAllText(destinationMetaData, File.ReadAllText(metaData));
                else
                    File.WriteAllText(destinationMetaData, MetaData);
            }
        }

        public static void SetBitness(ThunderKitSettings settings)
        {
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
        internal const string MetaData =
@"fileFormatVersion: 2
guid: fc64261ca6282254da01b0f016bcfcea
PluginImporter:
  externalObjects: {}
  serializedVersion: 2
  iconMap: {}
  executionOrder: {}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 0
  isExplicitlyReferenced: 1
  validateReferences: 1
  platformData:
  - first:
      '': Any
    second:
      enabled: 0
      settings:
        Exclude Editor: 0
        Exclude Linux: 0
        Exclude Linux64: 0
        Exclude LinuxUniversal: 0
        Exclude OSXUniversal: 0
        Exclude Win: 0
        Exclude Win64: 0
  - first:
      Any: 
    second:
      enabled: 1
      settings: {}
  - first:
      Editor: Editor
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
        DefaultValueInitialized: true
        OS: AnyOS
  - first:
      Facebook: Win
    second:
      enabled: 0
      settings:
        CPU: AnyCPU
  - first:
      Facebook: Win64
    second:
      enabled: 0
      settings:
        CPU: AnyCPU
  - first:
      Standalone: Linux
    second:
      enabled: 1
      settings:
        CPU: x86
  - first:
      Standalone: Linux64
    second:
      enabled: 1
      settings:
        CPU: x86_64
  - first:
      Standalone: LinuxUniversal
    second:
      enabled: 1
      settings: {}
  - first:
      Standalone: OSXUniversal
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
  - first:
      Standalone: Win
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
  - first:
      Standalone: Win64
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
  - first:
      Windows Store Apps: WindowsStoreApps
    second:
      enabled: 0
      settings:
        CPU: AnyCPU
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";
    }
}
#endif
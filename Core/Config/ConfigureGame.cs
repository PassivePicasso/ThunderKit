using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ThunderKit.Common.Package;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ThunderKit.Core.Config
{
    public class ConfigureGame
    {
        [MenuItem(Constants.ThunderKitMenuRoot + "Configure Game", priority = Constants.ThunderKitMenuPriority)]
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

            AssetDatabase.ImportAsset("Packages", ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
        }

        private static void SetupPackageManifest(ThunderKitSettings settings, string packageName)
        {
            var name = packageName.ToLower().Split(' ').Aggregate((a, b) => $"{a}{b}");
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(settings.GamePath, settings.GameExecutable));
            var unityVersion = new Version(fileVersionInfo.FileVersion);
            var gameVersion = new Version(fileVersionInfo.FileVersion);
            var author = new Author
            {
                name = fileVersionInfo.CompanyName,
            };
            var packageManifest = new PackageManagerManifest(author, name, packageName, "1.0.0", $"{unityVersion.Major}.{unityVersion.Minor}", $"Imported Assets from game {packageName}");
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
                {
                    using (var md5 = MD5.Create())
                    {
                        byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(Path.GetFileNameWithoutExtension(assembly)));
                        Guid result = new Guid(hash);
                        string guid = result.ToString().ToLower().Replace("-", "");
                        File.WriteAllText(destinationMetaData, MetaData(guid));
                    }
                }
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

        static string nl => Environment.NewLine;
        internal static string MetaData(string guid) =>
"fileFormatVersion: 2"
+ nl + $"guid: {guid}"
+ nl + "PluginImporter:"
+ nl + "  externalObjects: {}"
+ nl + "  serializedVersion: 2"
+ nl + "  iconMap: {}"
+ nl + "  executionOrder: {}"
+ nl + "  defineConstraints: []"
+ nl + "  isPreloaded: 0"
+ nl + "  isOverridable: 0"
+ nl + "  isExplicitlyReferenced: 1"
+ nl + "  validateReferences: 1"
+ nl + "  platformData:"
+ nl + "  - first:"
+ nl + "      '': Any"
+ nl + "    second:"
+ nl + "      enabled: 0"
+ nl + "      settings:"
+ nl + "        Exclude Editor: 0"
+ nl + "        Exclude Linux: 0"
+ nl + "        Exclude Linux64: 0"
+ nl + "        Exclude LinuxUniversal: 0"
+ nl + "        Exclude OSXUniversal: 0"
+ nl + "        Exclude Win: 0"
+ nl + "        Exclude Win64: 0"
+ nl + "  - first:"
+ nl + "      Any: "
+ nl + "    second:"
+ nl + "      enabled: 1"
+ nl + "      settings: {}"
+ nl + "  - first:"
+ nl + "      Editor: Editor"
+ nl + "    second:"
+ nl + "      enabled: 1"
+ nl + "      settings:"
+ nl + "        CPU: AnyCPU"
+ nl + "        DefaultValueInitialized: true"
+ nl + "        OS: AnyOS"
+ nl + "  - first:"
+ nl + "      Facebook: Win"
+ nl + "    second:"
+ nl + "      enabled: 0"
+ nl + "      settings:"
+ nl + "        CPU: AnyCPU"
+ nl + "  - first:"
+ nl + "      Facebook: Win64"
+ nl + "    second:"
+ nl + "      enabled: 0"
+ nl + "      settings:"
+ nl + "        CPU: AnyCPU"
+ nl + "  - first:"
+ nl + "      Standalone: Linux"
+ nl + "    second:"
+ nl + "      enabled: 1"
+ nl + "      settings:"
+ nl + "        CPU: x86"
+ nl + "  - first:"
+ nl + "      Standalone: Linux64"
+ nl + "    second:"
+ nl + "      enabled: 1"
+ nl + "      settings:"
+ nl + "        CPU: x86_64"
+ nl + "  - first:"
+ nl + "      Standalone: LinuxUniversal"
+ nl + "    second:"
+ nl + "      enabled: 1"
+ nl + "      settings: {}"
+ nl + "  - first:"
+ nl + "      Standalone: OSXUniversal"
+ nl + "    second:"
+ nl + "      enabled: 1"
+ nl + "      settings:"
+ nl + "        CPU: AnyCPU"
+ nl + "  - first:"
+ nl + "      Standalone: Win"
+ nl + "    second:"
+ nl + "      enabled: 1"
+ nl + "      settings:"
+ nl + "        CPU: AnyCPU"
+ nl + "  - first:"
+ nl + "      Standalone: Win64"
+ nl + "    second:"
+ nl + "      enabled: 1"
+ nl + "      settings:"
+ nl + "        CPU: AnyCPU"
+ nl + "  - first:"
+ nl + "      Windows Store Apps: WindowsStoreApps"
+ nl + "    second:"
+ nl + "      enabled: 0"
+ nl + "      settings:"
+ nl + "        CPU: AnyCPU"
+ nl + "  userData: "
+ nl + "  assetBundleName: "
+ nl + "  assetBundleVariant: ";
    }
}

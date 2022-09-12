using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ThunderKit.Common.Configuration;
using ThunderKit.Common.Package;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace ThunderKit.Core.Utilities
{
    public static class PackageHelper
    {
#if UNITY_2020_3_OR_NEWER
        public static void ResolvePackages() => Client.Resolve();
#elif UNITY_2019_3_OR_NEWER
        private static readonly MethodInfo ClientResolve = typeof(Client).GetMethod("Resolve", BindingFlags.NonPublic | BindingFlags.Static);
        public static void ResolvePackages() => ClientResolve.Invoke(null, null);
#else
        public static void ResolvePackages() => AssetDatabase.Refresh();
#endif

        static readonly Regex versionRegex = new Regex("(\\d+\\.\\d+\\.\\d+)(\\..*?)?");
        public static void GeneratePackageManifest(string packageName, string outputDir, string displayName, string authorAlias, string version, string description = null)
        {
            string unityVersion = Application.unityVersion.Substring(0, Application.unityVersion.LastIndexOf("."));
            var author = new Author
            {
                name = authorAlias,
            };
            string ver;
            var match = versionRegex.Match(version);
            if (match.Success)
                ver = match.Groups[1].Value;
            else
            {
                ver = "0.0.1";
                description = (description ?? string.Empty) + "\r\n\r\n(Version number may be inaccurate)";
            }

            var packageManifest = new PackageManagerManifest(author, packageName, ObjectNames.NicifyVariableName(displayName), ver, unityVersion, description);
            var packageManifestJson = JsonUtility.ToJson(packageManifest);
            ScriptingSymbolManager.AddScriptingDefine(packageName);

            string fullOutputPath = Path.Combine(outputDir, "package.json");
            if (File.Exists(fullOutputPath)) File.Delete(fullOutputPath);
            File.WriteAllText(fullOutputPath, packageManifestJson);
            File.SetLastWriteTime(fullOutputPath, DateTime.Now);
        }

        public static PackageManagerManifest GetPackageManagerManifest(string directory)
        {
            var packageJsonPath = Path.Combine(directory, "package.json");
            var json = File.ReadAllText(packageJsonPath);
            var pmm = JsonUtility.FromJson<PackageManagerManifest>(json);
            return pmm;
        }


        /// <summary>
        /// Generate a unity meta file for an assembly with ThunderKit managed Guid
        /// </summary>
        /// <param name="assemblyPath">Path to assembly to generate meta file for</param>
        /// <param name="metadataPath">Path to write meta file to</param>
        public static void WriteAssemblyMetaData(string assemblyPath, string metadataPath)
        {
            string guid = GetFileNameHash(assemblyPath);
            string metaData = DefaultAssemblyMetaData(guid);
            if (File.Exists(metadataPath)) File.Delete(metadataPath);
            File.WriteAllText(metadataPath, metaData);
            File.SetLastWriteTime(metadataPath, DateTime.Now);
        }

        /// <summary>
        /// Generate a unity meta file for an asset with ThunderKit managed Guid
        /// </summary>
        /// <param name="assemblyPath">Path to asset to generate meta file for</param>
        /// <param name="metadataPath">Path to write meta file to</param>
        public static void WriteAssetMetaData(string assetPath, string guid = null)
        {
            guid = guid ?? GetFileNameHash(assetPath);
            string metaData = DefaultScriptableObjectMetaData(guid);

            var metadataPath = $"{assetPath}.meta";
            if (File.Exists(metadataPath)) File.Delete(metadataPath);
            File.WriteAllText(metadataPath, metaData);
            File.SetLastWriteTime(metadataPath, DateTime.Now);
        }


        public static string GetFileNameHash(string assemblyPath)
        {
            string shortName = Path.GetFileNameWithoutExtension(assemblyPath);
            var settings = ThunderKitSetting.GetOrCreateSettings<ImportConfiguration>();
            var importAssemblies = settings.ConfigurationExecutors.OfType<ImportAssemblies>().First();
            string result;
            switch (importAssemblies.GuidGenerationMode)
            {
                case GuidMode.AssetRipperCompatibility:
                    result = GetAssetRipperStringHash(shortName);
                    break;
                case GuidMode.Stabilized:
                    result = GetStringHashUTF8(shortName);
                    break;
                case GuidMode.Original:
                default:
                    result = GetStringHash(shortName);
                    break;
            }
            return result;
        }

        public static string GetStringHash(string value)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.Default.GetBytes(value));
                var guid = new Guid(hashBytes);
                var cleanedGuid = guid.ToString().ToLower().Replace("-", "");
                return cleanedGuid;
            }
        }
        public static string GetStringHashUTF8(string value)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                var guid = new Guid(hashBytes);
                var cleanedGuid = guid.ToString().ToLower().Replace("-", "");
                return cleanedGuid;
            }
        }

        public static string GetAssetRipperStringHash(string value)
        {
            const byte VersionMask = 0xF0;
            const byte Md5GuidVersion = 0x30;
            const byte ClockSeqHiAndReservedMask = 0xC0;
            const byte ClockSeqHiAndReservedValue = 0x80;

            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value));

                // time_hi_and_version
                hashBytes[7] = (byte)(hashBytes[7] & ~VersionMask | Md5GuidVersion);
                // clock_seq_hi_and_reserved
                hashBytes[8] = (byte)(hashBytes[8] & ~ClockSeqHiAndReservedMask | ClockSeqHiAndReservedValue);

                var guid = new Guid(hashBytes);
                var cleanedGuid = guid.ToString().ToLower().Replace("-", "");
                return cleanedGuid;
            }
        }

        public static string DefaultScriptableObjectMetaData(string guid) =>
$@"fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";

        public static string DefaultAssemblyMetaData(string guid) =>
$@"fileFormatVersion: 2
guid: {guid}
PluginImporter:
  externalObjects: {{}}
  serializedVersion: 2
  iconMap: {{}}
  executionOrder: {{}}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 0
  isExplicitlyReferenced: 1
  validateReferences: 0
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
      settings: {{}}
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
      settings: {{}}
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
  assetBundleVariant: ";
    }
}
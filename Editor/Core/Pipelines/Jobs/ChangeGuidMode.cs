using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Pipelines.Jobs
{
    using static ThunderKit.Core.Data.ThunderKitSettings;
    using static PathExtensions;
    using Object = UnityEngine.Object;

    [PipelineSupport(typeof(Pipeline))]
    public class ChangeGuidMode : PipelineJob
    {
        public GuidMode oldGuidMode;
        public GuidMode newGuidMode;
        public DefaultAsset[] UnityObjects;
        public override Task Execute(Pipeline pipeline)
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
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            Dictionary<string, string> guidMaps = new Dictionary<string, string>();

            foreach (var installedAssembly in Directory.EnumerateFiles(settings.PackagePath, $"*.dll", SearchOption.TopDirectoryOnly))
            {
                var asmPath = installedAssembly.Replace("\\", "/");
                string assemblyFileName = Path.GetFileName(asmPath);
                var destinationMetaData = Combine(settings.PackagePath, $"{assemblyFileName}.meta");
                guidMaps[PackageHelper.GetFileNameHash(assemblyFileName, oldGuidMode)] = PackageHelper.GetFileNameHash(assemblyFileName, newGuidMode);
            }
            foreach (var installedAssembly in Directory.EnumerateFiles(settings.PackagePluginsPath, $"*.{nativeAssemblyExtension}", SearchOption.TopDirectoryOnly))
            {
                var asmPath = installedAssembly.Replace("\\", "/");
                string assemblyFileName = Path.GetFileName(asmPath);
                var destinationMetaData = Combine(settings.PackagePluginsPath, $"{assemblyFileName}.meta");
                guidMaps[PackageHelper.GetFileNameHash(assemblyFileName, oldGuidMode)] = PackageHelper.GetFileNameHash(assemblyFileName, newGuidMode);
            }

            var unityObjects = UnityObjects;
            var objectPaths = unityObjects.Select(AssetDatabase.GetAssetPath);
            var assetPaths = objectPaths
                .SelectMany(path =>
                {
                    if (AssetDatabase.IsValidFolder(path))
                        return Directory.GetFiles(path, "*.asset", SearchOption.AllDirectories);
                    else
                        return Enumerable.Repeat(path, 1);
                }).ToArray();

            foreach (var path in assetPaths)
            {
                var lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                    if (lines[i].Contains("m_Script"))
                        foreach (var oldGuid in guidMaps.Keys)
                            if (lines[i].Contains(oldGuid))
                                lines[i] = lines[i].Replace(oldGuid, guidMaps[oldGuid]);

                File.WriteAllLines(path, lines);
            }

            return Task.CompletedTask;
        }

        static string GetFileNameHash(string assemblyPath, GuidMode guidMode)
        {
            string shortName = Path.GetFileNameWithoutExtension(assemblyPath);
            string result;
            switch (guidMode)
            {
                case GuidMode.AssetRipperCompatibility:
                    result = PackageHelper.GetAssetRipperStringHash(shortName);
                    break;
                case GuidMode.Original:
                default:
                    result = PackageHelper.GetStringHash(shortName);
                    break;
            }
            Debug.Log($"Converting {assemblyPath} Identity to Mode:{guidMode} Value({result})");
            return result;
        }
    }
}
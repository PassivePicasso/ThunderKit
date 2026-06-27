using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Config
{
    /// <summary>
    /// Imports managed <em>stub</em> assemblies for IL2CPP games. IL2CPP games ship
    /// no managed assemblies, so <see cref="ImportAssemblies"/> (which copies the
    /// game's *_Data/Managed DLLs) has nothing to work with. Instead this importer
    /// recovers the game's types from il2cpp metadata into empty-bodied stub
    /// assemblies the Editor can author against - letting users place the game's own
    /// components on new prefabs/scenes and configure their serialized fields, which
    /// the game's native types deserialize at runtime.
    ///
    /// Generation is delegated to a pluggable <see cref="Il2CppStubGenerator"/>; this
    /// executor only selects a generator, runs it, and imports the result. For Mono
    /// games it is a no-op, so it can sit alongside <see cref="ImportAssemblies"/>.
    /// </summary>
    [Serializable]
    public class ImportIl2CppStubs : OptionalExecutor
    {
        // Discovers stub generators fresh each import. TypeCache is the reliable path
        // on 2019.2+ (AppDomain enumeration at load can throw on some assemblies during
        // the attribute scan); generators do not require [ImportExtensions].
        static List<Il2CppStubGenerator> DiscoverGenerators()
        {
#if UNITY_2019_2_OR_NEWER
            IEnumerable<Type> types = TypeCache.GetTypesDerivedFrom<Il2CppStubGenerator>();
#else
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm != null)
                .SelectMany(asm =>
                {
                    try { return asm.GetTypes(); }
                    catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
                });
#endif
            return types
                .Where(t => t != null && !t.IsAbstract && !t.IsInterface)
                .Where(t => typeof(Il2CppStubGenerator).IsAssignableFrom(t))
                .Select(t => CreateInstance(t) as Il2CppStubGenerator)
                .Where(g => g != null)
                .OrderByDescending(g => g.Priority)
                .ToList();
        }

        public override int Priority => Constants.Priority.Il2CppStubImport;
        public override string Description => "Recovers and imports managed stub assemblies for IL2CPP games so the game's own components can be authored onto prefabs and scenes.";

        public override bool Execute()
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();

            // Mono games are handled by ImportAssemblies; nothing to do here.
            if (!Il2CppUtility.IsIl2Cpp(settings))
                return true;

            var generators = DiscoverGenerators();
            var generator = generators.FirstOrDefault(g => g.CanGenerate(settings));
            if (generator == null)
            {
                if (generators.Count == 0)
                    Debug.LogWarning("[ThunderKit] No Il2CppStubGenerator implementations were found, so IL2CPP game types cannot be recovered.");
                else
                    Debug.LogWarning($"[ThunderKit] Found {generators.Count} Il2CppStubGenerator(s) but none can handle this game ({string.Join(", ", generators.Select(g => g.Name))}). Confirm the game is IL2CPP and the platform is supported.");
                return true;
            }

            var packagePath = settings.PackageFilePath;
            if (!Directory.Exists(packagePath))
                Directory.CreateDirectory(packagePath);

            var workDir = Path.Combine(Constants.TempDir, "Il2CppStubs", settings.PackageName);
            try { if (Directory.Exists(workDir)) Directory.Delete(workDir, true); } catch { }
            Directory.CreateDirectory(workDir);

            IReadOnlyList<string> stubs;
            try
            {
                if (!generator.TryGenerate(settings, workDir, out stubs) || stubs == null || stubs.Count == 0)
                {
                    Debug.LogWarning($"[ThunderKit] {generator.Name} produced no stub assemblies for {settings.PackageName}.");
                    return true;
                }
            }
            catch (Exception e)
            {
                // Surface the failure but don't wedge the import pipeline.
                Debug.LogError($"[ThunderKit] IL2CPP stub generation failed: {e}");
                return true;
            }

            try
            {
                AssetDatabase.StartAssetEditing();
                EditorApplication.LockReloadAssemblies();
                ImportStubs(packagePath, stubs);
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
                AssetDatabase.StopAssetEditing();
            }
            return true;
        }

        static void ImportStubs(string destinationFolder, IReadOnlyList<string> stubAssemblies)
        {
            foreach (var stubPath in stubAssemblies)
            {
                if (string.IsNullOrEmpty(stubPath) || !File.Exists(stubPath))
                    continue;

                var fileName = Path.GetFileName(stubPath);
                var destinationFile = Path.Combine(destinationFolder, fileName);
                var destinationMeta = Path.Combine(destinationFolder, $"{fileName}.meta");
                try
                {
                    if (File.Exists(destinationFile)) File.Delete(destinationFile);
                    File.Copy(stubPath, destinationFile);
                    PackageHelper.WriteAssemblyMetaData(stubPath, destinationMeta);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ThunderKit] Could not import stub assembly {fileName}: {e.Message}");
                }
            }
        }
    }
}

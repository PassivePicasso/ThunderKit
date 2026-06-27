using System;
using System.IO;
using ThunderKit.Core.Data;

namespace ThunderKit.Core.Utilities
{
    /// <summary>
    /// Helpers for locating and validating an IL2CPP game's native binary and type
    /// metadata. Used by the IL2CPP import path (<see cref="Config.ImportIl2CppStubs"/>
    /// and <see cref="Config.Il2CppStubGenerator"/> implementations) to decide whether
    /// a game's types can be recovered into editor stub assemblies.
    /// </summary>
    public static class Il2CppUtility
    {
        // global-metadata.dat begins with this magic when it is neither encrypted
        // nor obfuscated. A mismatch indicates XOR/obfuscated or decoy metadata that
        // cannot be dumped into stub assemblies without custom deobfuscation.
        public const uint MetadataMagic = 0xFAB11BAF;

        /// <summary>
        /// True when the configured game uses the IL2CPP scripting backend, detected
        /// by a GameAssembly native module or an il2cpp_data folder.
        /// </summary>
        public static bool IsIl2Cpp(ThunderKitSettings settings)
        {
            if (settings == null
             || string.IsNullOrEmpty(settings.GamePath)
             || string.IsNullOrEmpty(settings.GameExecutable))
                return false;

            if (FindGameAssembly(settings.GamePath) != null)
                return true;

            return Directory.Exists(Path.Combine(settings.GameDataPath, "il2cpp_data"));
        }

        /// <summary>
        /// Locates the IL2CPP native module (GameAssembly.dll/.so/.dylib) at the game
        /// root, or null when none is present.
        /// </summary>
        public static string FindGameAssembly(string gameDir)
        {
            if (string.IsNullOrEmpty(gameDir))
                return null;
            foreach (var name in new[] { "GameAssembly.dll", "GameAssembly.so", "GameAssembly.dylib" })
            {
                var path = Path.Combine(gameDir, name);
                if (File.Exists(path))
                    return path;
            }
            return null;
        }

        /// <summary>
        /// Locates global-metadata.dat under the game's *_Data/il2cpp_data/Metadata,
        /// or null when it is absent.
        /// </summary>
        public static string FindGlobalMetadata(string gameDataPath)
        {
            if (string.IsNullOrEmpty(gameDataPath))
                return null;
            var path = Path.Combine(Path.Combine(Path.Combine(gameDataPath, "il2cpp_data"), "Metadata"), "global-metadata.dat");
            return File.Exists(path) ? path : null;
        }

        /// <summary>
        /// True when global-metadata.dat has a valid magic and a plausible sanity
        /// version (roughly 16..31 across the Unity 5.x -> 6000 eras), meaning its
        /// types are recoverable into stub assemblies.
        /// </summary>
        public static bool HasReadableMetadata(string metadataPath)
        {
            if (string.IsNullOrEmpty(metadataPath) || !File.Exists(metadataPath))
                return false;
            try
            {
                using (var fs = File.OpenRead(metadataPath))
                {
                    var header = new byte[8];
                    if (fs.Read(header, 0, header.Length) < header.Length)
                        return false;
                    var magic = BitConverter.ToUInt32(header, 0);
                    var sanityVersion = BitConverter.ToInt32(header, 4);
                    return magic == MetadataMagic && sanityVersion >= 16 && sanityVersion <= 31;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

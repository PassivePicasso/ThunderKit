using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.IO;

namespace ThunderKit.Core.Utilities
{
    // Unity players ship their serialized files either loose in <Game>_Data or, when
    // built with chunk based compression, packed into a single data.unity3d bundle.
    internal static class PlayerDataResolver
    {
        internal const string GlobalGameManagers = "globalgamemanagers";
        internal const string CompressedPlayerData = "data.unity3d";

        // Loose layout first: a player only ships one of these.
        static readonly string[] PlayerDataFileNames = { GlobalGameManagers, CompressedPlayerData };

        // A bundle that holds no globalgamemanagers. Distinct from an unreadable file so
        // callers can skip a layout we do not understand rather than fail the import.
        internal class MissingGlobalGameManagersException : Exception
        {
            public MissingGlobalGameManagersException(string message) : base(message) { }
        }

        internal static bool TryGetPlayerDataPath(string gameDataPath, out string playerDataPath)
        {
            playerDataPath = null;
            if (string.IsNullOrEmpty(gameDataPath))
                return false;

            foreach (var fileName in PlayerDataFileNames)
            {
                var candidate = Path.Combine(gameDataPath, fileName);
                if (!File.Exists(candidate))
                    continue;

                playerDataPath = candidate;
                return true;
            }

            return false;
        }

        // playerDataPath is either a loose globalgamemanagers or a bundle containing one.
        internal static AssetsFileInstance LoadGlobalGameManagers(AssetsManager assetsManager, string playerDataPath)
        {
            if (assetsManager == null)
                throw new ArgumentNullException(nameof(assetsManager));
            if (!File.Exists(playerDataPath))
                throw new FileNotFoundException($"No player data at '{playerDataPath}'.", playerDataPath);

            if (AssetsFile.IsAssetsFile(playerDataPath))
                return assetsManager.LoadAssetsFile(playerDataPath, true);

            var bundle = assetsManager.LoadBundleFile(playerDataPath, true);
            var index = FindGlobalGameManagers(bundle.file);
            if (index < 0)
                throw new MissingGlobalGameManagersException(
                    $"'{playerDataPath}' contains no '{GlobalGameManagers}' entry " +
                    $"(found: {string.Join(", ", bundle.file.GetAllFileNames())}).");

            // Returns null rather than throwing when the entry is not a serialized file.
            var globalGameManagers = assetsManager.LoadAssetsFileFromBundle(bundle, index, true);
            if (globalGameManagers == null)
                throw new MissingGlobalGameManagersException(
                    $"the '{GlobalGameManagers}' entry in '{playerDataPath}' is not a readable serialized file.");

            return globalGameManagers;
        }

        // Compressed players answer from the bundle header, so nothing is decompressed
        // to satisfy what is only a version question.
        internal static bool TryGetPlayerUnityVersion(string playerDataPath, out string unityVersion)
        {
            unityVersion = null;
            if (string.IsNullOrEmpty(playerDataPath) || !File.Exists(playerDataPath))
                return false;

            try
            {
                unityVersion = AssetsFile.IsAssetsFile(playerDataPath)
                    ? ReadAssetsFileVersion(playerDataPath)
                    : ReadBundleVersion(playerDataPath);
            }
            catch
            {
                unityVersion = null;
            }

            return !string.IsNullOrEmpty(unityVersion);
        }

        static string ReadAssetsFileVersion(string path)
        {
            var assetsManager = new AssetsManager();
            try
            {
                return assetsManager.LoadAssetsFile(path, false).file.Metadata.UnityVersion;
            }
            finally
            {
                assetsManager.UnloadAll(true);
            }
        }

        static string ReadBundleVersion(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var bundle = new AssetBundleFile();
                bundle.Read(new AssetsFileReader(stream));
                try
                {
                    return bundle.Header.EngineVersion;
                }
                finally
                {
                    bundle.Close();
                }
            }
        }

        // GetFileIndex matches ordinally; the sweep guards against a layout that spells
        // the entry differently to the ones observed so far.
        static int FindGlobalGameManagers(AssetBundleFile bundle)
        {
            var index = bundle.GetFileIndex(GlobalGameManagers);
            if (index >= 0)
                return index;

            var directories = bundle.BlockAndDirInfo.DirectoryInfos;
            for (var i = 0; i < directories.Count; i++)
                if (GlobalGameManagers.Equals(directories[i].Name, StringComparison.OrdinalIgnoreCase))
                    return i;

            return -1;
        }
    }
}

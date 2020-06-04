#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Utilities;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace PassivePicasso.ThunderKit.Deploy.Editor
{
    public class SetupDebugBuild : NetworkBehaviour
    {
        private const string playerConnectionDebug1 = "player-connection-debug=1";

        [MenuItem("Tools/ThunderKit/Setup Debug Build")]
        public static void Execute()
        {
            var settings = ThunderKitSettings.GetOrCreateSettings();

            var gamePath = settings.GamePath;
            var gameDir = new DirectoryInfo(gamePath);
            var gameName = gameDir.Name;
            var gameMonoPath = Path.Combine(gamePath, $"MonoBleedingEdge");
            var gameDataPath = Path.Combine(gamePath, $"{gameName}_Data");
            var gameManagedPath = Path.Combine(gameDataPath, "Managed");
            var gameBootConfigFile = Path.Combine(gameDataPath, "boot.config");

            var editorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
            var windowsStandalonePath = Path.Combine(editorPath, "Data", "PlaybackEngines", "windowsstandalonesupport");
            var bit64Path = Path.Combine(windowsStandalonePath, "Variations", "win64_development_mono");
            var monoBleedingEdgePath = Path.Combine(bit64Path, "MonoBleedingEdge");
            var dataManagedPath = Path.Combine(bit64Path, "Data", "Managed");

            var (winPlayer, gamePlayer) = GetSwapPair(bit64Path, gamePath, "WindowsPlayer.exe", $"{gameName}.exe");
            var (unityCrashHandler, rorCrashHandler) = GetSwapPair(bit64Path, gamePath, "UnityCrashHandler64.exe");
            var (unityPlayer, rorUnityPlayer) = GetSwapPair(bit64Path, gamePath, "UnityPlayer.dll");
            var (unityPlayerLib, rorUnityPlayerLib) = GetSwapPair(bit64Path, gamePath, "UnityPlayer.dll.lib");
            var (unityPlayerDpdb, rorUnityPlayerDpdb) = GetSwapPair(bit64Path, gamePath, "UnityPlayer_Win64_development_mono_x64.pdb");
            var (unityPlayerRpdb, rorUnityPlayerRpdb) = GetSwapPair(bit64Path, gamePath, "WindowsPlayer_Release_mono_x64.pdb");
            var (winPixDll, rorwinPixDll) = GetSwapPair(bit64Path, gamePath, "WinPixEventRuntime.dll");

            var editorVersion = FileVersionInfo.GetVersionInfo(winPlayer);
            var gameVersion = FileVersionInfo.GetVersionInfo(gamePlayer);

            if (!editorVersion.Equals(editorVersion))
            {
                Debug.LogError($"Unity Editor Version: {editorVersion} does not match {settings.GameExecutable} Unity version: {gameVersion}");
                return;
            }

            Overwrite(winPlayer, gamePlayer);
            Overwrite(unityCrashHandler, rorCrashHandler);
            Overwrite(unityPlayer, rorUnityPlayer);
            Overwrite(unityPlayerLib, rorUnityPlayerLib);
            Overwrite(unityPlayerDpdb, rorUnityPlayerDpdb);
            Overwrite(unityPlayerRpdb, rorUnityPlayerRpdb);
            Overwrite(winPixDll, rorwinPixDll);

            CopyFolder(monoBleedingEdgePath, gameMonoPath);
            CopyFolder(dataManagedPath, gameManagedPath);

            if (File.Exists(gameBootConfigFile))
            {
                var bootConfig = File.ReadAllLines(gameBootConfigFile).ToList();
                if (!bootConfig.Any(line => line.Contains(playerConnectionDebug1)))
                    bootConfig.Add(playerConnectionDebug1);

                File.WriteAllLines(gameBootConfigFile, bootConfig);
            }
            else
                File.WriteAllText(gameBootConfigFile, playerConnectionDebug1);
        }

        private static (string sourcePath, string destinationPath) GetSwapPair(string sourceRoot, string destRoot, string sourceFilename, string destinationFilename = null)
        {
            return (Path.Combine(sourceRoot, sourceFilename), Path.Combine(destRoot, destinationFilename ?? sourceFilename));
        }

        private static void Overwrite(string newFile, string originalFile)
        {
            if (File.Exists(originalFile)) File.Delete(originalFile);
            File.Copy(newFile, originalFile);
        }

        private static void CopyFolder(string sourcePath, string destinationPath)
        {
            foreach (var dir in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(Path.Combine(destinationPath, dir.Substring(sourcePath.Length + 1)));

            foreach (var fileName in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                var original = Path.Combine(destinationPath, fileName.Substring(sourcePath.Length + 1));
                Overwrite(fileName, original);
            }

        }

    }
}
#endif
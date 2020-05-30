#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using RainOfStages.AutoConfig;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Networking;
using System.Linq;

namespace RainOfStages.Deploy
{
    public class SetupDebugBuild : NetworkBehaviour
    {
        private const string playerConnectionDebug1 = "player-connection-debug=1";

        [MenuItem("Tools/Rain of Stages/Setup Debug Build")]
        public static void Execute()
        {
            var settings = ThunderKitSettings.GetOrCreateSettings();

            var rorPath = settings.GamePath;
            var rorDir = new DirectoryInfo(rorPath);
            var rorName = rorDir.Name;
            var rorMonoPath = Path.Combine(rorPath, $"MonoBleedingEdge");
            var rorDataPath = Path.Combine(rorPath, $"{rorName}_Data");
            var rorManagedPath = Path.Combine(rorDataPath, "Managed");
            var rorBootConfigFile = Path.Combine(rorDataPath, "boot.config");

            var editorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
            var windowsStandalonePath = Path.Combine(editorPath, "Data", "PlaybackEngines", "windowsstandalonesupport");
            var bit64Path = Path.Combine(windowsStandalonePath, "Variations", "win64_development_mono");
            var monoBleedingEdgePath = Path.Combine(bit64Path, "MonoBleedingEdge");
            var dataManagedPath = Path.Combine(bit64Path, "Data", "Managed");

            var (winPlayer, rorPlayer) = GetSwapPair(bit64Path, rorPath, "WindowsPlayer.exe", $"{rorName}.exe");
            var (unityCrashHandler, rorCrashHandler) = GetSwapPair(bit64Path, rorPath, "UnityCrashHandler64.exe");
            var (unityPlayer, rorUnityPlayer) = GetSwapPair(bit64Path, rorPath, "UnityPlayer.dll");
            var (unityPlayerLib, rorUnityPlayerLib) = GetSwapPair(bit64Path, rorPath, "UnityPlayer.dll.lib");
            var (unityPlayerDpdb, rorUnityPlayerDpdb) = GetSwapPair(bit64Path, rorPath, "UnityPlayer_Win64_development_mono_x64.pdb");
            var (unityPlayerRpdb, rorUnityPlayerRpdb) = GetSwapPair(bit64Path, rorPath, "WindowsPlayer_Release_mono_x64.pdb");
            var (winPixDll, rorwinPixDll) = GetSwapPair(bit64Path, rorPath, "WinPixEventRuntime.dll");

            var editorVersion = FileVersionInfo.GetVersionInfo(winPlayer);
            var rorVersion = FileVersionInfo.GetVersionInfo(rorPlayer);

            if (!editorVersion.Equals(editorVersion))
            {
                Debug.LogError($"Unity Editor Version: {editorVersion} does not match Risk of Rain 2 Unity version: {rorVersion}");
                return;
            }

            Overwrite(winPlayer, rorPlayer);
            Overwrite(unityCrashHandler, rorCrashHandler);
            Overwrite(unityPlayer, rorUnityPlayer);
            Overwrite(unityPlayerLib, rorUnityPlayerLib);
            Overwrite(unityPlayerDpdb, rorUnityPlayerDpdb);
            Overwrite(unityPlayerRpdb, rorUnityPlayerRpdb);
            Overwrite(winPixDll, rorwinPixDll);

            CopyFolder(monoBleedingEdgePath, rorMonoPath);
            CopyFolder(dataManagedPath, rorManagedPath);

            if (File.Exists(rorBootConfigFile))
            {
                var bootConfig = File.ReadAllLines(rorBootConfigFile).ToList();
                if (!bootConfig.Any(line => line.Contains(playerConnectionDebug1)))
                    bootConfig.Add(playerConnectionDebug1);

                File.WriteAllLines(rorBootConfigFile, bootConfig);
            }
            else
                File.WriteAllText(rorBootConfigFile, playerConnectionDebug1);
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
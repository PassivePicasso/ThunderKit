using AssetsTools.NET.Extra;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ThunderKit.Core.Config
{
    using static ThunderKit.Common.PathExtensions;

    [Serializable]
    public class GameConfigurator
    {
        public static async Task LoadGame(ThunderKitSettings settings)
        {
            if (string.IsNullOrEmpty(settings.GamePath) || string.IsNullOrEmpty(settings.GameExecutable))
            {
                LocateGame(settings);
                return;
            }

            if (!CheckUnityVersion(settings)) return;

            foreach (var executor in settings.ConfigurationExecutors)
                switch (executor)
                {
                    case OptionalExecutor optional:
                        if (optional.enabled)
                            await optional.Execute();
                        break;
                    default:
                        await executor.Execute();
                        break;
                }

            SetBitness(settings);

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
            AssetDatabase.ImportAsset(settings.PackageFilePath);
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
                    case RuntimePlatform.LinuxEditor:
                        path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "*");
                        break;
                    //case RuntimePlatform.OSXEditor:
                    //    path = EditorUtility.OpenFilePanel("Open Game Executable", currentDir, "app");
                    //    break;
                    default:
                        EditorUtility.DisplayDialog("Unsupported", "Your operating system is partially or completely unsupported. Contributions to improve this are welcome", "Ok");
                        return;
                }
                if (string.IsNullOrEmpty(path)) return;
                //For Linux, we will have to check the selected file to see if the GameExecutable_Data folder exists, we can use this to verify the executable was selected
                settings.GameExecutable = Path.GetFileName(path);
                settings.GamePath = Path.GetDirectoryName(path);
                foundExecutable = Directory.GetFiles(settings.GamePath, settings.GameExecutable).Any();
            }
            EditorUtility.SetDirty(settings);
        }

        private static bool CheckUnityVersion(ThunderKitSettings settings)
        {
            var versionMatch = false;
            var regs = new Regex(".*?(\\d{1,4}\\.\\d+\\.\\d+\\w\\d+).*");

            var unityVersion = regs.Replace(Application.unityVersion, match => match.Groups[1].Value);

            var informationFile = Combine(settings.GameDataPath, "globalgamemanagers");
            var playerVersion = string.Empty;
            if (!File.Exists(informationFile))
            {
                informationFile = Combine(settings.GameDataPath, "data.unity3d");
            }
            if (File.Exists(informationFile))
            {
                var am = new AssetsManager();
                var ggm = am.LoadAssetsFile(informationFile, false);

                playerVersion = ggm.table.file.typeTree.unityVersion;

                am.UnloadAll(true);

                versionMatch = unityVersion.Equals(playerVersion);
            }
            else
            {
                var exePath = Combine(settings.GamePath, settings.GameExecutable);
                var fvi = FileVersionInfo.GetVersionInfo(exePath);
                playerVersion = fvi.FileVersion.Substring(0, fvi.FileVersion.LastIndexOf("."));
                if (playerVersion.Count(f => f == '.') == 2)
                    versionMatch = unityVersion.Equals(playerVersion);
            }

            var result = versionMatch ? "" : ", aborting setup.\r\n\t Make sure you're using the same version of the Unity Editor as the Unity Player for the game.";
            Debug.Log($"Unity Editor version ({unityVersion}), Unity Player version ({playerVersion}){result}");
            return versionMatch;
        }

        public static void SetBitness(ThunderKitSettings settings)
        {
            if (Application.platform != RuntimePlatform.WindowsEditor) return;
            var assembly = Combine(settings.GamePath, settings.GameExecutable);
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

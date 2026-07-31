using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ThunderKit.Core.Data;
using ThunderKit.Core.Utilities;
using UnityEngine;

namespace ThunderKit.Core.Config.Common
{
    public class CheckUnityVersion : OptionalExecutor
    {
        public override int Priority => int.MaxValue - 50;

        public override string Description => "Validate that the version of the Unity Editor matches the version of the game. " +
            "This should not be disabled but is available for debugging.  Importing a game with an unmatched version of Unity is unsupported. Disable at your own risk.";

        public override bool Execute()
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
            var versionMatch = false;
            var regs = new Regex("(\\d{1,4}\\.\\d+\\.\\d+)(.*)");

            var unityVersion = regs.Replace(Application.unityVersion, match => match.Groups[1].Value);
            var playerVersion = string.Empty;

            bool foundVersion = false;

            if (PlayerDataResolver.TryGetPlayerDataPath(settings.GameDataPath, out var informationFile)
                && PlayerDataResolver.TryGetPlayerUnityVersion(informationFile, out var serializedVersion))
            {
                playerVersion = regs.Replace(serializedVersion, match => match.Groups[1].Value);
                versionMatch = unityVersion.Equals(playerVersion);
                foundVersion = true;
            }

            if (!foundVersion)
            {
                // Non-Windows executables carry no version resource, so FileVersion is null there.
                var fvi = FileVersionInfo.GetVersionInfo(Path.Combine(settings.GamePath, settings.GameExecutable));
                playerVersion = regs.Replace(fvi.FileVersion ?? string.Empty, match => match.Groups[1].Value);
                if (playerVersion.Count(f => f == '.') == 2)
                    versionMatch = unityVersion.Equals(playerVersion);
            }

            if (!versionMatch)
            {
                throw new System.Exception($"Unity Editor version ({unityVersion}), Unity Player version ({playerVersion}), aborting setup." +
                        $"\r\n\t Make sure you're using the same version of the Unity Editor as the Unity Player for the game.");
            }
            return true;
        }
    }
}
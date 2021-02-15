using System.IO;
using UnityEditor;

namespace ThunderKit.Integrations.Thunderstore
{
    internal static class Constants
    {
        public const string ThunderKit = nameof(ThunderKit);

        public const string ThunderStorePath = Common.Constants.ThunderKitContextRoot + "Thunderstore/";

        public static readonly string TempDir = Path.Combine(Directory.GetCurrentDirectory(), "Temp", ThunderKit);
        public static readonly string Packages = Path.Combine("Packages");

        [InitializeOnLoadMethod]
        static void SetupTempDir()
        {
            Directory.CreateDirectory(TempDir);
        }
    }
}
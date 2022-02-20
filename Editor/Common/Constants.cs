using System.IO;
using UnityEditor;

namespace ThunderKit.Common
{
    public static class Constants
    {
        public const string ThunderKit = nameof(ThunderKit);

        public const int ThunderKitMenuPriority = 18;
        public const string ThunderKitContextRoot = "Assets/ThunderKit/";
        public const string ThunderKitMenuRoot = "Tools/ThunderKit/";
        public const string ThunderKitSettingsRoot = "Assets/ThunderKitSettings/";

        public static readonly string TempDir = PathExtensions.Combine(Directory.GetCurrentDirectory(), "Temp", ThunderKit);
        public static readonly string Packages = "Packages";
        public static readonly string ThunderKitPackageName = "com.passivepicasso.thunderkit";

        public static readonly string[] FindAllFolders = new[] { "Packages", "Assets" };
        public static readonly string[] FindAssetsFolders = new[] { "Assets" };
        public static readonly string[] FindPackagesFolders = new[] { "Packages" };

        public const string DocumentationStylePath = "Packages/com.passivepicasso.thunderkit/uss/thunderkit_style.uss";
        public const string SettingsTemplatesPath = "Packages/com.passivepicasso.thunderkit/Editor/Core/Templates/Settings";
        public const string PackageSourceSettingsTemplatePath = SettingsTemplatesPath + "/PackageSourceSettings.uxml";
        public const string ThunderKitSettingsTemplatePath = SettingsTemplatesPath + "/ThunderKitSettings.uxml";
        public const string ThunderKitSkinPath = "Packages/com.passivepicasso.thunderkit/Editor/Skins/ThunderKitSkin.guiskin";

        [InitializeOnLoadMethod]
        static void SetupTempDir()
        {
            Directory.CreateDirectory(TempDir);
        }
    }
}
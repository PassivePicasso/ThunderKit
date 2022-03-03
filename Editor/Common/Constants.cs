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

        public const string ThunderKitRoot = "Packages/com.passivepicasso.thunderkit";
        public const string DocumentationStylePath = ThunderKitRoot + "/USS/thunderkit_style.uss";
        public const string SettingsTemplatesPath = ThunderKitRoot + "/UXML/Settings";
        public const string PackageSourceSettingsTemplatePath = SettingsTemplatesPath + "/PackageSourceSettings.uxml";
        public const string ThunderKitSettingsTemplatePath = SettingsTemplatesPath + "/ThunderKitSettings.uxml";

        public static readonly string ClassDataPath = Path.Combine("Packages", "com.passivepicasso.thunderkit", "Editor", "ThirdParty", "AssetsTools.NET", "classdata.tpk");

        public static class Icons
        {
            public const string ManifestIconPath = ThunderKitRoot + "/Graphics/Rasters/Icons/TK_Manifest_Icon.png";
            public const string PipelineIconPath = ThunderKitRoot + "/Graphics/Rasters/Icons/TK_Pipeline_Icon.png";
            public const string DocumentationIconPath = ThunderKitRoot + "/Graphics/Rasters/Icons/TK_Documentation_Icon.png";
            public const string PackageSourceIconPath = ThunderKitRoot + "/Graphics/Rasters/Icons/TK_PackageSource_Icon.png";
            public const string PathReferenceIconPath = ThunderKitRoot + "/Graphics/Rasters/Icons/TK_PathReference_Icon.png";
            public const string SettingIconPath = ThunderKitRoot + "/Graphics/Rasters/Icons/TK_Setting_Icon.png";
        }

        [InitializeOnLoadMethod]
        static void SetupTempDir()
        {
            Directory.CreateDirectory(TempDir);
        }
    }
}
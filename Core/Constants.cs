using ThunderKit.Common.Configuration;
using UnityEditor;

namespace ThunderKit.Core
{
    public static class Constants
    {
        public const int ThunderKitMenuPriority = 18;
        public const string ThunderKitContextRoot = "Assets/ThunderKit/";
        public const string ThunderKitMenuRoot = "ThunderKit/";

        public static readonly string[] AssetDatabaseFindFolders = new[] { "Packages", "Assets" };

        [InitializeOnLoadMethod]
        static void DefineInstalled()
        {
            ScriptingSymbolManager.AddScriptingDefine("ThunderKitInstalled");
        }
    }
}
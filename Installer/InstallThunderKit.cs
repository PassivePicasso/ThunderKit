#if !ThunderKitInstalled
#if !IsThunderKitProject
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;

namespace ThunderKit.Installer
{
    public class InstallThunderKit
    {
        [InitializeOnLoadMethod]
        static void InstallThunderKitNow()
        {
            var listRequest = Client.List(true);
            if (listRequest != null && listRequest.Result != null)
                foreach (var package in listRequest.Result)
                    if (package.packageId.StartsWith("com.passivepicasso.thunderkit@https://github.com/PassivePicasso/ThunderKit.git"))
                    {
                        return;
                    }

            AssetDatabase.StartAssetEditing();
            if (AssetDatabase.IsValidFolder("Assets/ThunderKit/Installer"))
            {
                AssetDatabase.DeleteAsset("Assets/ThunderKit/Installer");
                if (!Directory.EnumerateFiles("Assets/ThunderKit", "*", SearchOption.AllDirectories).Any())
                    AssetDatabase.DeleteAsset("Assets/ThunderKit");
            }
            AddScriptingDefine("ThunderKitInstalled");
            Client.Add("https://github.com/PassivePicasso/ThunderKit.git");
            AssetDatabase.StopAssetEditing();
        }
        
        static bool IsObsolete(BuildTargetGroup group)
        {
            var attrs = typeof(BuildTargetGroup).GetField(group.ToString()).GetCustomAttributes(typeof(ObsoleteAttribute), false);
            return attrs.Length > 0;
        }

        internal static bool ContainsDefine(string define)
        {
            foreach (BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

                if (!defineSymbols.Contains(define))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Add a define to the scripting define symbols for every build target.
        /// </summary>
        /// <param name="define"></param>
        public static void AddScriptingDefine(string define)
        {
            foreach (BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

                if (!defineSymbols.Contains(define))
                {
                    if (defineSymbols.Length < 1)
                        defineSymbols = define;
                    else if (defineSymbols.EndsWith(";"))
                        defineSymbols = string.Format("{0}{1}", defineSymbols, define);
                    else
                        defineSymbols = string.Format("{0};{1}", defineSymbols, define);

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
                }
            }
        }

        /// <summary>
        /// Remove a define from the scripting define symbols for every build target.
        /// </summary>
        /// <param name="define"></param>
        public static void RemoveScriptingDefine(string define)
        {
            foreach (BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

                if (defineSymbols.Contains(define))
                {
                    defineSymbols = defineSymbols.Replace(string.Format("{0};", define), "");
                    defineSymbols = defineSymbols.Replace(define, "");

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
                }
            }
        }
    }
}
#endif
#endif
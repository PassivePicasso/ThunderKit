using System;
using UnityEditor;

namespace ThunderKit.Common.Configuration
{
    public static class ScriptingSymbolManager
    {
        static bool IsObsolete(BuildTargetGroup group)
        {
            var attrs = typeof(BuildTargetGroup).GetField(group.ToString()).GetCustomAttributes(typeof(ObsoleteAttribute), false);
            return attrs.Length > 0;
        }

        public static bool ContainsDefine(string define)
        {
            define = define.ToUpperInvariant();
            define = define.Replace("-", "_");

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
            define = define.ToUpperInvariant();
            define = define.Replace("-", "_").Replace(".", "_");

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
            define = define.ToUpperInvariant();
            define = define.Replace("-", "_");

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
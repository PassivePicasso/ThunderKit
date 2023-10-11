using System.IO;
using UnityEditor;

namespace ThunderKit.Core.Config.Common
{
    public class DisableAssemblyUpdater : OptionalExecutor
    {
        public override int Priority => int.MaxValue - 100;

        public override string Description => "Prompt the user to Restart Unity to disable the Assembly. Disabling the updater is recommended and is required in some cases. If the import processes seems to never end, the fix is usually to disable the Assembly Updater.";
        private bool isRestarting = false;

        public override bool Execute()
        {
            var args = System.Environment.GetCommandLineArgs();
            var promptRestart = true;
            for (int i = 0; i < args.Length; i++)
                if (args[i] == "-disable-assembly-updater")
                    promptRestart = false;

            if (isRestarting)
            {
                if (promptRestart)
                    return false;
                isRestarting = false;
                return true;
            }

            if (promptRestart)
            {
                var restart = EditorUtility.DisplayDialog(
                          title: "Disable Assembly Updater",
                        message: "Disabling the Unity Automatic Assembly Updater is recommended as game assemblies should not be updated. Disabling the updater will reduce import times. Disabling the Assembly Updater requires the project to restart.",
                             ok: "Restart Project",
                         cancel: "No Thanks");

                if (restart)
                {
                    isRestarting = true;
                    EditorApplication.OpenProject(Directory.GetCurrentDirectory(), "-disable-assembly-updater");
                    return false;
                }
            }
            return true;
        }
    }
}
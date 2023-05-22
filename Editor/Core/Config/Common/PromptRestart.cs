using System.IO;
using UnityEditor;

namespace ThunderKit.Core.Config.Common
{
    public class PromptRestart : OptionalExecutor
    {
        public override int Priority => int.MinValue;

        public override string Description => "Prompt the user to Restart Unity at the end of the import cycle";

        public override bool Execute()
        {
            var restart = EditorUtility.DisplayDialog(
                          title: "Import Process Complete",
                        message: "The game has been imported successfully. It is recommended to restart your project to ensure stability",
                             ok: "Restart Project",
                         cancel: "Restart Later");

            if (restart)
                EditorApplication.OpenProject(Directory.GetCurrentDirectory());

            return true;
        }
    }
}
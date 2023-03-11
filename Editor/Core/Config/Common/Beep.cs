using UnityEditor;

namespace ThunderKit.Core.Config.Common
{
    public class Beep : OptionalExecutor
    {
        public override int Priority => int.MinValue;

        public override string Description => "Invokes UnityEditor.EditorApplication.Beep() to play the system default beep";

        public override bool Execute()
        {
            EditorApplication.Beep();
            return true;
        }
    }
}
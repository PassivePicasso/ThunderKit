using UnityEditor;

namespace ThunderKit.Core.Config.Common
{
    public class Beep : OptionalExecutor
    {
        public override int Priority => int.MinValue;

        public override bool Execute()
        {
            EditorApplication.Beep();
            return true;
        }
    }
}
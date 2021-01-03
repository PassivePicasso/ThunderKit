#if UNITY_EDITOR
using System;
using UnityEditor.ProjectWindowCallback;

namespace PassivePicasso.ThunderKit.Gui
{
    public class SelfDestructingActionAsset : EndNameEditAction
    {
        public Action<int, string, string> action;

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            action(instanceId, pathName, resourceFile);
            CleanUp();
        }
    }
}
#endif
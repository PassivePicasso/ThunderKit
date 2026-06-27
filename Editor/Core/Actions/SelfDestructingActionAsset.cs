using System;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace ThunderKit.Core.Actions
{
    #if UNITY_6000_4_OR_NEWER
    public class SelfDestructingActionAsset : AssetCreationEndAction
    {
        public Action<EntityId, string, string> action;

        public override void Action(EntityId instanceId, string pathName, string resourceFile)
        {
            action(instanceId, pathName, resourceFile);
            CleanUp();
        }
    }
    #else
    public class SelfDestructingActionAsset : EndNameEditAction
    {
        public Action<int, string, string> action;

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            action(instanceId, pathName, resourceFile);
            CleanUp();
        }
    }
    #endif
}

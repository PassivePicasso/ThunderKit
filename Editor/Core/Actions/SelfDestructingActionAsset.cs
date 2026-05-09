using System;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace ThunderKit.Core.Actions
{
    #if UNITY_6000_5_OR_NEWER
    class SelfDestructingActionAsset : AssetCreationEndAction
    {
        public Action<EntityId, string, string> action;

        public override void Action(EntityId instanceId, string pathName, string resourceFile)
        {
            action(instanceId, pathName, resourceFile);
            CleanUp();
        }
    }
    #else
    class SelfDestructingActionAsset : EndNameEditAction
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

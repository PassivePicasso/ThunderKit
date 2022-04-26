using System;
using ThunderKit.Common;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using UnityEditor;

namespace ThunderKit.Addressable.Tools
{
    public abstract class AddressableGraphicsImport : OptionalExecutor
    {
        public override int Priority => Constants.Priority.AddressableGraphicsImport;

        public virtual string CustomDeferredReflection => null;
        public virtual string CustomDeferredScreenspaceShadows => null;
        public virtual string CustomDeferredShading => null;

        public sealed override void Execute()
        {
            var settingsType = Type.GetType("ThunderKit.Addressable.Tools.AddressableGraphicsSettings, ThunderKit.Addressable.Tools, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (settingsType == null)
                throw new ArgumentException("ThunderKit.Addressable.Tools.AddressableGraphicsSettings not found");

            var settings = ThunderKitSetting.GetOrCreateSettings(settingsType) as ThunderKitSetting;
            var settingsSo = new SerializedObject(settings);

            var customDeferredReflection = settingsSo.FindProperty("CustomDeferredReflection");
            var customDeferredScreenspaceShadows = settingsSo.FindProperty("CustomDeferredScreenspaceShadows");
            var customDeferredShading = settingsSo.FindProperty("CustomDeferredShading");

            customDeferredReflection.stringValue = CustomDeferredReflection;
            customDeferredScreenspaceShadows.stringValue = CustomDeferredScreenspaceShadows;
            customDeferredShading.stringValue = CustomDeferredShading;

            settingsSo.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
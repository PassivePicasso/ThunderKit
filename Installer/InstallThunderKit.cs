#if !THUNDERKIT_CONFIGURED
#if !IsThunderKitProject
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PassivePicasso.RainOfStages.Installer
{
    [InitializeOnLoad]
    public class InstallThunderKit
    {
        static InstallThunderKit()
        {
            Client.Add("https://github.com/PassivePicasso/ThunderKit.git");
        }
    }
}
#endif
#endif
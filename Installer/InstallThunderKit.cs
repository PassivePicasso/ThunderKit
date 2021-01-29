#if !ThunderKitInstalled
#if !IsThunderKitProject
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;

namespace PassivePicasso.RainOfStages.Installer
{
    public class InstallThunderKit
    {
        [InitializeOnLoadMethod]
        static void InstallThunderKitNow()
        {
            var listRequest = Client.List(true);
            if (listRequest != null && listRequest.Result != null)
                foreach (var package in listRequest.Result)
                    if (package.packageId == "com.passivepicasso.thunderkit@https://github.com/PassivePicasso/ThunderKit.git#development")
                    {
                        return;
                    }

            Client.Add("https://github.com/PassivePicasso/ThunderKit.git#development");
            if (Directory.Exists("Assets/ThunderKit/Installer"))
            {
                Directory.Delete("Assets/ThunderKit/Installer", true);
                File.Delete("Assets/ThunderKit/Installer.meta");

                if (!Directory.EnumerateFiles("Assets/ThunderKit", "*", SearchOption.AllDirectories).Any())
                    Directory.Delete("Assets/ThunderKit", true);

                AssetDatabase.Refresh();
            }
            AssetDatabase.Refresh();
        }
    }
}
#endif
#endif
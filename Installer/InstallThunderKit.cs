#if !ThunderKitInstalled
#if !IsThunderKitProject
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;

namespace ThunderKit.Installer
{
    public class InstallThunderKit
    {
        [InitializeOnLoadMethod]
        static void InstallThunderKitNow()
        {
            var listRequest = Client.List(true);
            if (listRequest != null && listRequest.Result != null)
                foreach (var package in listRequest.Result)
                    if (package.packageId.StartsWith("com.passivepicasso.thunderkit@https://github.com/PassivePicasso/ThunderKit.git"))
                    {
                        return;
                    }

            AssetDatabase.StartAssetEditing();
            if (AssetDatabase.IsValidFolder("Assets/ThunderKit/Installer"))
            {
                AssetDatabase.DeleteAsset("Assets/ThunderKit/Installer");
                if (!Directory.EnumerateFiles("Assets/ThunderKit", "*", SearchOption.AllDirectories).Any())
                    AssetDatabase.DeleteAsset("Assets/ThunderKit");
            }
            Client.Add("https://github.com/PassivePicasso/ThunderKit.git");
            AssetDatabase.StopAssetEditing();
        }
    }
}
#endif
#endif
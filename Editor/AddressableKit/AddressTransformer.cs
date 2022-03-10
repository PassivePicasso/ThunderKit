using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.AddressableAssets;
using UnityEditor;
using System.IO;

namespace ThunderKit.RemoteAddressables
{
    static class AddressTransformer
    {
        //Implement a method to transform the internal ids of locations
        static string MyCustomTransform(IResourceLocation location)
        {
            var path = location.InternalId.Replace("\\", "/");
            var standardPwd = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "StreamingAssets", "aa").Replace("\\", "/");
            if (location.ResourceType == typeof(IAssetBundleResource) && path.StartsWith(standardPwd))
                path = path.Replace(standardPwd, ThunderKit.Core.Data.ThunderKitSettings.EditTimePath);

            return path;
        }

        //Override the Addressables transform method with your custom method.
        //This can be set to null to revert to default behavior.
        [InitializeOnLoadMethod]
        static void SetInternalIdTransform()
        {
            Addressables.InternalIdTransformFunc = MyCustomTransform;
        }
    }
}

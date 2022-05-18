using UnityEditor;

namespace ThunderKit.Core.Data
{
    public class PackageSourceProcessor : UnityEditor.AssetModificationProcessor
    {
        static AssetDeleteResult OnWillDeleteAsset(string assetName, RemoveAssetOptions options)
        {
            var type = AssetDatabase.GetMainAssetTypeAtPath(assetName);
            if (typeof(PackageSource).IsAssignableFrom(type))
            {
                var ps = AssetDatabase.LoadAssetAtPath<PackageSource>(assetName);
                PackageSourceSettings.UnregisterSource(ps);
            }
            return AssetDeleteResult.DidNotDelete;
        }
    }
}
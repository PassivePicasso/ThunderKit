using System.IO;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Utilities
{
    using static ScriptableHelper;
    public class BundleAsset : ScriptableObject
    {

        [MenuItem(ThunderKitContextRoot + nameof(BundleAsset), true)]
        public static bool CanCreate() => ".manifest".Equals(Path.GetExtension(AssetDatabase.GetAssetPath(Selection.activeObject)));

        [MenuItem(ThunderKitContextRoot + nameof(BundleAsset), false)]
        public static void Create()
        {
            CreateAsset<BundleAsset>(() => Selection.activeObject.name);
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var directoryPath = Path.GetDirectoryName(path);
            var outputDirName = $"{Selection.activeObject.name}-Assets";
            var bundleAssetDir = Path.Combine(directoryPath, outputDirName);
            var bundlePath = Path.Combine(directoryPath, Path.GetFileNameWithoutExtension(path));
            if (!AssetDatabase.IsValidFolder(bundleAssetDir)) AssetDatabase.CreateFolder(directoryPath, outputDirName);

            var bundle = AssetBundle.LoadFromFile(bundlePath);
            var contents = bundle.LoadAllAssets();
            foreach (var content in contents)
            {
                var duplicate = Object.Instantiate(content);
                duplicate.name = content.name;

                var assetTypeDir = Path.Combine(bundleAssetDir, content.GetType().Name);
                if (!AssetDatabase.IsValidFolder(assetTypeDir)) AssetDatabase.CreateFolder(bundleAssetDir, content.GetType().Name);
                try
                {
                    switch (duplicate)
                    {
                        case GameObject contentObject:
                            PrefabUtility.SaveAsPrefabAsset(contentObject, Path.Combine(assetTypeDir, $"{content.name}.prefab"));
                            break;
                        default:
                            AssetDatabase.CreateAsset(duplicate, Path.Combine(assetTypeDir, $"{content.name}.asset"));
                            break;
                    }
                }
                catch { }
            }
            bundle.Unload(true);
            
        }
    }
}

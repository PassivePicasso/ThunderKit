#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Utilities
{
    using static ScriptableHelper;
    public class BundleAsset : ScriptableObject
    {
        private const string BundleAssetMenuPath = ThunderKitContextRoot + nameof(BundleAsset);

        [MenuItem(BundleAssetMenuPath, true)]
        public static bool CanCreate() => ".manifest".Equals(Path.GetExtension(AssetDatabase.GetAssetPath(Selection.activeObject)));

        [MenuItem(BundleAssetMenuPath, false)]
        public static void Create()
        {
            SelectNewAsset<BundleAsset>(() => Selection.activeObject.name);
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
#endif
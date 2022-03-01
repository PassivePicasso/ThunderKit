using System.IO;
using System.Linq;
using ThunderKit.Common;
using ThunderKit.Core.Attributes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThunderKit.Core.Data
{
    using static ScriptableHelper;

    public class UnityPackage : ScriptableObject
    {
        const string ExportMenuPath = Constants.ThunderKitContextRoot + "Compile " + nameof(UnityPackage);

        [EnumFlag]
        public IncludedSettings IncludedSettings;
        [EnumFlag]
        public ExportPackageOptions exportPackageOptions = ExportPackageOptions.Recurse;

        public Object[] AssetFiles;

        [MenuItem(Constants.ThunderKitContextRoot + nameof(UnityPackage), false, priority = Constants.ThunderKitMenuPriority)]
        public static void Create()
        {
            SelectNewAsset<UnityPackage>();
        }

        public void Export(string path)
        {
            var assetPaths = AssetFiles.Select(af => AssetDatabase.GetAssetPath(af));
            var additionalAssets = IncludedSettings.GetFlags().Select(flag => $"ProjectSettings/{flag}.asset");
            assetPaths = assetPaths.Concat(additionalAssets);

            string[] assetPathNames = assetPaths.ToArray();
            string fileName = Path.Combine(path, $"{name}.unityPackage");
            string metaFileName = Path.Combine(path, $"{name}.unityPackage.meta");
            if (File.Exists(fileName)) File.Delete(fileName);
            if (File.Exists(metaFileName)) File.Delete(metaFileName);
            AssetDatabase.ExportPackage(assetPathNames, fileName, exportPackageOptions);
        }
    }
}
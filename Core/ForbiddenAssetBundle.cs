using ThunderKit.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core
{
    public class ForbiddenAssetBundle : ScriptableObject
    {
        [MenuItem(Constants.ThunderKitContextRoot + "Forbidden AssetBundle", priority = Constants.ThunderKitMenuPriority)]
        public static void Create() => ScriptableHelper.SelectNewAsset<ForbiddenAssetBundle>();
    }
}
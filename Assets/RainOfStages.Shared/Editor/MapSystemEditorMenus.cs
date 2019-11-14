#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace RainOfStages.Editor
{
    public class MapSystemEditorMenus : ScriptableObject
    {
        [MenuItem("Tools/Map System/Clear AssetBundles")]
        static void DoIt()








        {
            var allBundleNAmes = AssetDatabase.GetAllAssetBundleNames();
            foreach (var name in allBundleNAmes)
            {
                Debug.Log(name);
                AssetDatabase.RemoveAssetBundleName(name, true);
            }

        }
    }
}
#endif
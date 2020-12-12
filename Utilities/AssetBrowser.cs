#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace PassivePicasso.ThunderKit.Utilities
{
    public class AssetBrowser : EditorWindow
    {
        private const string AssetBrowserTemplateName = "AssetBrowser";
        VisualElement rootVisualElement;

        //AssetBundle current;
        static FileSystemWatcher fileSystemWatcher;
        static AssetBrowser assetBrowser;

        [MenuItem(ScriptableHelper.ThunderKitMenuRoot + "Asset Browser", priority = 0)]
        public static void ShowDefaultWindow() => Rebuild();

        static volatile bool RebuildWnd = false;


        [InitializeOnLoadMethod]
        private static void AssertFileWatcher()
        {
            if (fileSystemWatcher == null)
            {
                fileSystemWatcher = new FileSystemWatcher();
                var (templateRoot, assetPath, sheetPath) = GetPaths();
                fileSystemWatcher.Path = templateRoot;
                fileSystemWatcher.Filter = $"{AssetBrowserTemplateName}.*";
                fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            }
        }

        private static void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e) => RebuildWnd = true;

        private void OnGUI()
        {
            if (RebuildWnd)
            {
                Rebuild();
                RebuildWnd = false;
            }
        }

        private void OnEnable() => fileSystemWatcher.EnableRaisingEvents = true;

        private void OnDisable() => fileSystemWatcher.EnableRaisingEvents = false;

        private static void Rebuild()
        {
            if (assetBrowser == null)
            {
                assetBrowser = GetWindow<AssetBrowser>();
                assetBrowser.titleContent = new GUIContent("Assets");
                assetBrowser.rootVisualElement = assetBrowser.GetRootVisualContainer();
            }
            assetBrowser.rootVisualElement.Clear();

            var (_, assetPath, sheetPath) = GetPaths();

            var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
            var element = uiAsset.CloneTree(null);
            element.name = "AssetBrowserRoot";
            assetBrowser.rootVisualElement.Add(element);

            assetBrowser.rootVisualElement.AddStyleSheetPath(sheetPath);

            //assetBrowser.OnSelectionChange();
        }

        private static (string templateRoot, string assetPath, string sheetPath) GetPaths()
        {
            string templateRoot = Path.Combine("Assets", "ThunderKit", "Utilities", "UXML");
            string sheetPath = Path.Combine(templateRoot, $"{AssetBrowserTemplateName}.USS");
            string assetPath = Path.Combine(templateRoot, $"{AssetBrowserTemplateName}.UXML");
            return (templateRoot, assetPath, sheetPath);
        }

        //public void OnSelectionChange()
        //{
        //    if (Selection.activeObject)
        //    {
        //        var so = new SerializedObject(Selection.activeObject);
        //        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        //        if (path.EndsWith(".manifest"))
        //        {
        //            var data = System.IO.File.ReadAllLines(path);
        //            if (data != null && data.Length > 0)
        //                if (data[0].StartsWith("ManifestFileVersion:"))
        //                {
        //                    //var bundlePath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));

        //                    //current = AssetBundle.LoadFromFile(bundlePath);
        //                    //var contents = current.LoadAllAssets();
        //                    //foreach (var content in contents)
        //                    //{
        //                    //}
        //                    //var textures = current.LoadAllAssets<Texture2D>();
        //                    //if (textures.Any())
        //                    //{
        //                    //}
        //                }
        //        }

        //        //m_ObjectNameBinding.Bind(so);
        //    }
        //    else
        //    {
        //        foreach (var ab in AssetBundle.GetAllLoadedAssetBundles()) ab?.Unload(true);
        //        //current?.Unload(true);
        //        //current = null;
        //        //m_ObjectNameBinding.Unbind();
        //        //m_ObjectNameBinding.value = "";
        //    }
        //}
    }
}
#endif
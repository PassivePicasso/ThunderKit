using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using VisualTemplates;

namespace ThunderKit.PackageManager
{
    public class ThunderKitPackageManager : EditorWindow
    {
        [MenuItem("Window/UIElements/ThunderKitPackageManager")]
        public static void ShowExample()
        {
            ThunderKitPackageManager wnd = GetWindow<ThunderKitPackageManager>();
            wnd.titleContent = new GUIContent("ThunderKitPackageManager");
        }

        public void OnEnable()
        {
            var root = this.GetRootVisualContainer();

            root.userData = this;
            ContentPresenter child = new ContentPresenter { userData = this, LoadAsset = LoadTemplate };
            root.Add(child);
        }

        private UnityEngine.Experimental.UIElements.VisualTreeAsset LoadTemplate(string dataType)
        {
            var template = AssetDatabase.FindAssets($"{dataType}.uxml").FirstOrDefault();
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(template);
        }
    }
}
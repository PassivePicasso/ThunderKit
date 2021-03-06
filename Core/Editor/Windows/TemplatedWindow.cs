using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Common;
using UnityEditor;
using UnityEngine;
using System.Reflection;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#elif UNITY_2018_1_OR_NEWER
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Editor.Windows
{
    public class TemplatedWindow : EditorWindow
    {
#if UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
        PropertyInfo rvcField;
        VisualElement rvc;
        protected VisualElement rootVisualContainer
        {
            get
            {
                if (rvcField == null)
                    rvcField = typeof(EditorWindow)
                               .GetProperty(nameof(rootVisualContainer), BindingFlags.NonPublic | BindingFlags.Instance);

                if(rvc == null)
                    rvc = rvcField.GetValue(this) as VisualElement;

                return rvc;
            }
        }
#endif
        private readonly static string[] SearchFolders = new string[] { "Assets", "Packages" };

        [SerializeField] public Texture2D ThunderKitIcon;

        Dictionary<string, VisualTreeAsset> templateCache = new Dictionary<string, VisualTreeAsset>();

        protected static string NicifyPackageName(string name) => ObjectNames.NicifyVariableName(name).Replace("_", " ");

        protected VisualElement GetTemplateInstance(string template, VisualElement target = null)
        {
            var packageTemplate = LoadTemplate(template);
            var assetPath = AssetDatabase.GetAssetPath(packageTemplate);
            VisualElement instance = target;
            if (instance == null) instance = packageTemplate.CloneTree(null);
            else
                packageTemplate.CloneTree(instance, null);

            instance.AddToClassList("grow");

#if UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
            AddSheet(instance, assetPath);
            if (EditorGUIUtility.isProSkin)
                AddSheet(instance, assetPath, "_Dark");
            else
                AddSheet(instance, assetPath, "_Light");
#endif
            return instance;
        }

        protected void AddSheet(VisualElement element, string assetPath, string modifier = "")
        {
            string sheetPath = assetPath.Replace(".uxml", $"{modifier}.uss");
            if (File.Exists(sheetPath)) element.AddStyleSheetPath(sheetPath);
        }

        protected VisualTreeAsset LoadTemplate(string name)
        {
            if (!templateCache.ContainsKey(name))
            {
                var searchResults = AssetDatabase.FindAssets(name, SearchFolders);
                var assetPaths = searchResults.Select(AssetDatabase.GUIDToAssetPath);
                var assetsRoot = Path.Combine("Assets", "ThunderKit");
                var packagesRoot = Path.Combine("Packages", Constants.ThunderKitPackageName);
                var templatePath = assetPaths
                    .Where(path => Path.GetFileNameWithoutExtension(path).Equals(name))
                    .Where(path => Path.GetExtension(path).Equals(".uxml", StringComparison.CurrentCultureIgnoreCase))
                    .Where(path => path.StartsWith(assetsRoot) || path.StartsWith(packagesRoot)
                                || path.StartsWith(assetsRoot.Replace("\\", "/")) || path.StartsWith(packagesRoot.Replace("\\", "/")))
                                             .FirstOrDefault(path => path.Contains("Templates/") || path.Contains("Templates\\"));
                templateCache[name] = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(templatePath);
            }
            return templateCache[name];
        }
    }
}
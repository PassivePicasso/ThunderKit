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
    public abstract class TemplatedWindow : EditorWindow
    {
#if UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
        PropertyInfo rvcField;
        VisualElement rvc;
        protected VisualElement rootVisualElement
        {
            get
            {
                if (rvcField == null)
                    rvcField = typeof(EditorWindow)
                               .GetProperty(nameof(rootVisualElement), BindingFlags.NonPublic | BindingFlags.Instance);

                if (rvc == null)
                    rvc = rvcField.GetValue(this) as VisualElement;

                return rvc;
            }
        }
#endif
        protected virtual Func<string, bool> IsTemplatePath
        {
            get
            {
                return path =>
                {
                    return Path.GetFileNameWithoutExtension(path) != "Templates" && path.Contains("Templates");
                };
            }
        }

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

            AddSheet(instance, assetPath);
            AddSheet(instance, assetPath, "_style");
            if (EditorGUIUtility.isProSkin)
                AddSheet(instance, assetPath, "_Dark");
            else
                AddSheet(instance, assetPath, "_Light");
            return instance;
        }

        public virtual void OnEnable()
        {
            rootVisualElement.Clear();
            GetTemplateInstance(GetType().Name, rootVisualElement);
            rootVisualElement.Bind(new SerializedObject(this));
        }

        protected void AddSheet(VisualElement element, string assetPath, string modifier = "")
        {
#if UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
            string sheetPath = assetPath.Replace(".uxml", $"{modifier}.uss");
            if (File.Exists(sheetPath)) element.AddStyleSheetPath(sheetPath);
#endif
        }

        protected VisualTreeAsset LoadTemplate(string name)
        {
            if (!templateCache.ContainsKey(name))
            {
                var searchResults = AssetDatabase.FindAssets(name, SearchFolders);
                var assetPaths = searchResults.Select(AssetDatabase.GUIDToAssetPath).Select(path => path.Replace("\\", "/"));
                var packagesRoot = Path.Combine("Packages", Constants.ThunderKitPackageName);
                var templatePath = assetPaths
                    .Where(path => Path.GetFileNameWithoutExtension(path).Equals(name))
                    .Where(path => Path.GetExtension(path).Equals(".uxml", StringComparison.CurrentCultureIgnoreCase))
                    .Where(IsTemplatePath)
                    .FirstOrDefault();
                templateCache[name] = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(templatePath);
            }
            return templateCache[name];
        }
    }
}
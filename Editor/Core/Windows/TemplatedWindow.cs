using System;
using System.IO;
using ThunderKit.Common;
using UnityEditor;
using UnityEngine;
using ThunderKit.Core.Actions;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
#elif UNITY_2018_1_OR_NEWER
using System.Reflection;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Windows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;

    public abstract class TemplatedWindow : EditorWindow
    {
#if UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
        PropertyInfo rvcField;
        VisualElement rvc;
#pragma warning disable IDE1006 // Naming Styles This is a compatibility layer and the casing is required
        protected VisualElement rootVisualElement
#pragma warning restore IDE1006 // Naming Styles
        {
            get
            {
                if (rvcField == null)
                    rvcField = typeof(EditorWindow)
                               .GetProperty("rootVisualContainer", BindingFlags.NonPublic | BindingFlags.Instance);

                if (rvc == null)
                    rvc = rvcField.GetValue(this, null) as VisualElement;

                return rvc;
            }
        }
#endif
        public virtual string Title { get => ObjectNames.NicifyVariableName(GetType().Name); }

        public Texture2D ThunderKitIcon;

        public virtual void OnEnable()
        {
            rootVisualElement.Clear();
            GetTemplateInstance(GetType().Name, rootVisualElement);
            titleContent = new GUIContent(Title, ThunderKitIcon);
            rootVisualElement.Bind(new SerializedObject(this));
        }

        #region TemplatedWindowHelper
        readonly static object[] findTextureParams = new object[1];

        [MenuItem(Constants.ThunderKitContextRoot + "Templated Window", priority = Constants.ThunderKitMenuPriority)]
        public static void CreateWindow()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"NewTemplatedWindow.cs");

            Action<int, string, string> action =
                (int instanceId, string pathname, string resourceFile) =>
                {
                    var name = Path.GetFileNameWithoutExtension(pathname);
                    var title = ObjectNames.NicifyVariableName(name);
                    var uxmlPathname = Path.ChangeExtension(pathname, ".uxml");
                    string rootElementName = $"{title.Replace(" ", "-").ToLowerInvariant()}-area";
                    File.WriteAllText(pathname, GetScriptTemplate(name, title, rootElementName));
                    File.WriteAllText(uxmlPathname, GetUXMLTemplate(rootElementName));
                    AssetDatabase.Refresh();
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(pathname);
                };

            var endAction = CreateInstance<SelfDestructingActionAsset>();
            endAction.action = action;
            var findTexture = typeof(EditorGUIUtility).GetMethod(nameof(EditorGUIUtility.FindTexture), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            findTextureParams[0] = typeof(MonoScript);
            var icon = (Texture2D)findTexture.Invoke(null, findTextureParams);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, endAction, assetPathAndName, icon, null);
        }

        static string GetScriptTemplate(string name, string title, string rootElementName) =>
$@"using System.Linq;
using ThunderKit.Common;
using ThunderKit.Core.Data;
using ThunderKit.Core.Editor.Windows;
using UnityEditor;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Windows
{{
    using static ThunderKit.Core.UIElements.TemplateHelpers;
    public class {name}: TemplatedWindow
    {{
        [MenuItem(Constants.ThunderKitMenuRoot + ""{title}"")]
        public static void Show{name}() => GetWindow<{name}>();

        public override string Title {{ get => ObjectNames.NicifyVariableName(GetType().Name); }}

        public override void OnEnable()
        {{
            base.OnEnable();

            var {name}Area = rootVisualElement.Q(""{rootElementName}"");
        }}
    }}
}}
";
        static string GetUXMLTemplate(string rootElementName) =>
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<engine:UXML
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
    xmlns:engine=""UnityEngine.Experimental.UIElements""
    xmlns:editor=""UnityEditor.Experimental.UIElements""
    xmlns:upm=""UnityEditor.PackageManager.UI""
>
  <engine:VisualElement name=""{rootElementName}"" >
  </engine:VisualElement>

</engine:UXML>";
        #endregion

    }
}
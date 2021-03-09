using ThunderKit.Common;
using ThunderKit.Core.Editor.Windows;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEditor.Experimental.UIElements;

namespace ThunderKit.Core.Windows
{
    using static TemplateHelpers;

    public class DocumentationWindow : TemplatedWindow
    {
        [MenuItem(Constants.ThunderKitMenuRoot + "Documentation")]
        public static void ShowDocumentation() => GetWindow<DocumentationWindow>();

        public override void OnEnable()
        {
            titleContent = new UnityEngine.GUIContent("ThunderKit Documentation", ThunderKitIcon);
            rootVisualElement.Clear();
            var element = LoadTemplateRelative(this, "../Documentation/DocumentationWindow");
            element.AddToClassList("grow");
            rootVisualElement.Add(element);
            rootVisualElement.Bind(new SerializedObject(this));
        }
    }
}
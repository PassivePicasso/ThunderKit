using ThunderKit.Core;
using ThunderKit.Core.Utilities;
using UnityEditor;

namespace ThunderKit.Core.Inspectors
{
    [CustomEditor(typeof(ComposableElement), true)]
    public class ComposableElementEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            try
            {
                DrawPropertiesExcluding(serializedObject, new string[] { "m_Script" });
            }
            catch { }
        }
    }
}
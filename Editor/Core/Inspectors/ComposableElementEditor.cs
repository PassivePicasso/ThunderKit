using UnityEditor;

namespace ThunderKit.Core.Inspectors
{
    [CustomEditor(typeof(ComposableElement), true)]
    public class ComposableElementEditor : Editor
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
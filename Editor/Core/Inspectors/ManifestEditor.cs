using System.Collections.Generic;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Inspectors
{
    [CustomEditor(typeof(Manifest), true)]
    public class ManifestEditor : ComposableObjectEditor
    {

        protected override IEnumerable<string> ExcludedProperties()
        {
            yield return nameof(Manifest.QuickAccess);
        }
        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
            var manifest = target as Manifest;

            var width = 100;
            var rect = new Rect(46, 22, width, 15);
            var cvw = EditorGUIUtility.currentViewWidth;
            var quickAccessContent = new GUIContent("Quick Access");
            var titleContent = new GUIContent(manifest.name);
            if (cvw < 350)
            {
                var titleContentSize = GUI.skin.button.CalcSize(titleContent);
                rect.x += titleContentSize.x;
                rect.y = 6;
            }

            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();

            EditorGUI.BeginChangeCheck();
            manifest.QuickAccess = GUI.Toggle(rect, manifest.QuickAccess, quickAccessContent);
            if (EditorGUI.EndChangeCheck())
            {
                settings.SetQuickAccess(manifest, manifest.QuickAccess);
                EditorUtility.SetDirty(manifest);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
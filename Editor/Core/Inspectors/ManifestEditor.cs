using System.Collections.Generic;
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
            manifest.QuickAccess = GUI.Toggle(rect, manifest.QuickAccess, "Quick Access");
        }
    }
}
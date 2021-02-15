using System.Collections;
using System.Collections.Generic;
using ThunderKit.Common;
using ThunderKit.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Manifests
{
    public class ManifestCollection : ScriptableObject, IEnumerable<Manifest>
    {
        [MenuItem(Constants.ThunderKitContextRoot + nameof(ManifestCollection), false, priority = Constants.ThunderKitMenuPriority)]
        public static void CreateComposableManifestPipeline() => ScriptableHelper.SelectNewAsset<ManifestCollection>();

        public Manifest[] manifests;

        public Manifest this[int index] => manifests[index];

        public int Length => manifests.Length;

        public IEnumerator<Manifest> GetEnumerator() => ((IEnumerable<Manifest>)manifests).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => manifests.GetEnumerator();
    }
}
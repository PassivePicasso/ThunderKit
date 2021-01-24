using PassivePicasso.ThunderKit.Core.Editor;
using PassivePicasso.ThunderKit.Core.Manifests;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace PassivePicasso.ThunderKit.Core.Pipelines
{
    public class ComposableManifestPipeline : Pipeline
    {
        [MenuItem(Constants.ThunderKitContextRoot + nameof(ComposableManifestPipeline), false, priority = Constants.ThunderKitMenuPriority)]
        public static void CreateComposableManifestPipeline() => ScriptableHelper.SelectNewAsset<ComposableManifestPipeline>();

        public Manifest[] manifests;
        public IEnumerable<ManifestDatum> Datums => manifests.SelectMany(manifest => manifest.Datum);

    }
}
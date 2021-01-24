using System;
using System.Linq;
using UnityEditor;
using static PassivePicasso.ThunderKit.Core.Editor.ScriptableHelper;

namespace PassivePicasso.ThunderKit.Core.Manifests
{
    public class Manifest : ComposableObject
    {
        public ManifestDatum[] Datum => base.Data.Cast<ManifestDatum>().ToArray();


        [MenuItem(Constants.ThunderKitContextRoot + nameof(Manifest), priority = Constants.ThunderKitMenuPriority)]
        public static void CreateManifest()
        {
            SelectNewAsset<Manifest>();
        }
        public override Type ElementType => typeof(ManifestDatum);

        public override string ElementTemplate => @"
using PassivePicasso.ThunderKit.Core.Manifests;

namespace {0}
{{
    public class {1} : ManifestDatum
    {{
    }}
}}
";

        public override bool SupportsType(Type type) => ElementType.IsAssignableFrom(type);


    }
}
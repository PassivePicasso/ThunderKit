using System;
using ThunderKit.Common;
using UnityEditor;
using static ThunderKit.Core.Editor.ScriptableHelper;

namespace ThunderKit.Core.Manifests
{
    public class Manifest : ComposableObject
    {
        [MenuItem(Constants.ThunderKitContextRoot + nameof(Manifest), priority = Constants.ThunderKitMenuPriority)]
        public static void Create()
        {
            SelectNewAsset<Manifest>();
        }
        public override Type ElementType => typeof(ManifestDatum);

        public override string ElementTemplate => @"
using ThunderKit.Core.Manifests;

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
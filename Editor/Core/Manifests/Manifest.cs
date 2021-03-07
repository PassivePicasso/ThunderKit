using System;
using System.Collections.Generic;
using ThunderKit.Common;
using ThunderKit.Core.Manifests.Datum;
using UnityEditor;
using UnityEngine;
using static ThunderKit.Core.Editor.ScriptableHelper;

namespace ThunderKit.Core.Manifests
{
    public class Manifest : ComposableObject
    {
        [MenuItem(Constants.ThunderKitContextRoot + nameof(Manifest), priority = Constants.ThunderKitMenuPriority)]
        public static void Create()
        {
            SelectNewAsset(afterCreated: (Action<Manifest>)(manifest =>
            {
                manifest.Identity = CreateInstance<ManifestIdentity>();
                manifest.Identity.name = nameof(ManifestIdentity);
                manifest.InsertElement(manifest.Identity, 0);
            }));
        }

        public IEnumerable<Manifest> EnumerateManifests()
        {
            if (this?.Identity?.Dependencies == null) yield break;

            HashSet<Manifest> returned = new HashSet<Manifest>();
            foreach (var dependency in this.Identity.Dependencies)
                foreach (var depManifest in dependency.EnumerateManifests())
                    if (returned.Add(depManifest))
                        yield return depManifest;

            if (returned.Add(this))
                yield return this;
        }

        [HideInInspector]
        public ManifestIdentity Identity;

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
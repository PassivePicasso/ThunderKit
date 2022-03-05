using System;
using System.Collections.Generic;
using System.Linq;

using ThunderKit.Common;
using ThunderKit.Core.Manifests.Datum;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static ThunderKit.Core.ScriptableHelper;

namespace ThunderKit.Core.Manifests
{
    public class Manifest : ComposableObject
    {
        [SerializeField, HideInInspector] private ManifestIdentity identity;

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
            if (!Identity)
                throw new ArgumentException($"[{name}.Identity](assetlink://{UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(this))}) is missing its identity.");
            if (Identity.Dependencies == null)
                throw new ArgumentException($"[{name}.Identity.Dependencies](assetlink://{UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(this))}) is null");
            if (Identity.Dependencies.Any(dep => !dep))
                throw new ArgumentException($"[{name}.Identity.Dependencies](assetlink://{UnityWebRequest.EscapeURL(AssetDatabase.GetAssetPath(this))}) has an unassigned entry");

            var returned = new HashSet<Manifest>(this.Identity.Dependencies.SelectMany(x => x.EnumerateManifests()));
            returned.Add(this);
            foreach (var ret in returned)
                yield return ret;
        }

        public ManifestIdentity Identity
        {
            get
            {
                if (identity == null)
                {
                    var path = AssetDatabase.GetAssetPath(this);
                    var reps = AssetDatabase.LoadAllAssetsAtPath(path);
                    var so = new SerializedObject(this);
                    var identProperty = so.FindProperty("identity");
                    identProperty.objectReferenceValue = reps.OfType<ManifestIdentity>().FirstOrDefault();
                    so.SetIsDifferentCacheDirty();
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                return identity;
            }

            set => identity = value;
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

        public static string GeneratePlainTextManifest(string author, string name, string description, string version, params string[] dependencyGuids) => $@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &-5155633585708372029
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f22bb7fd1d3b56a48bc52f8e407901d6, type: 3}}
  m_Name: {name}
  m_EditorClassIdentifier: 
  Errored: 0
  ErrorMessage: 
  ErrorStacktrace: 
  StagingPaths: []
  Author: {author}
  Name: {name}
  Description: {description}
  Version: {version}
  Dependencies: {dependencyGuids.Aggregate((dependencyGuids.Length == 0 ? "[]" : string.Empty), (a, b) => $"{a}\r\n{b}")}
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: e563561e08b6a3642a487177f5cfbb37, type: 3}}
  m_Name: {name}
  m_EditorClassIdentifier: 
  Data:
  - {{fileID: -5155633585708372029}}
  identity: {{fileID: -5155633585708372029}}";
    }
}
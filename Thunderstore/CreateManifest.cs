#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Core;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PassivePicasso.ThunderKit.Thunderstore
{
    using static ScriptableHelper;
    public class CreateManifest
    {
#pragma warning disable 0649
        public struct ManifestStub
        {
            public string name;
            public string author;
            public string version_number;
            public string website_url;
            public string description;
            public string[] dependencies;
        }
#pragma warning restore 0649

        [MenuItem(ThunderKitContextRoot + nameof(Manifest), true)]
        public static bool CanCreate()
        {
            if (Selection.activeObject is TextAsset)
            {
                var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                var filename = Path.GetFileName(assetPath);
                bool isManifest = "manifest.json".Equals(filename);
                try
                {
                    var json = File.ReadAllText(assetPath);
                    var stubManifest = JsonUtility.FromJson<ManifestStub>(json);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    isManifest = false;
                }

                return isManifest;
            }
            return true;
        }


        [MenuItem(ThunderKitContextRoot + nameof(Manifest), false)]
        public static void Create()
        {
            try
            {
                var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                FileAttributes attr = File.GetAttributes(absolutePath);

                var json = string.Empty;
                var name = "Manifest";
                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    LoadStub(assetPath);
                    json = File.ReadAllText(assetPath);
                    var stubManifest = JsonUtility.FromJson<ManifestStub>(json);
                    name = stubManifest.name;
                }

                SelectNewAsset<Manifest>(() => name);
                var instance = Selection.activeObject as Manifest;

                JsonUtility.FromJsonOverwrite(json, instance);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static ManifestStub LoadStub(string path) => JsonUtility.FromJson<ManifestStub>(File.ReadAllText(path));

    }
}
#endif
using System;
using System.IO;
using ThunderKit.Core.Editor;
using ThunderKit.Integrations.Thunderstore.Manifests;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Integrations.Thunderstore
{
    using static ScriptableHelper;
    public class CreateThunderstoreManifest
    {
#pragma warning disable 0649
        public struct ThunderstoreManifestStub
        {
            public string name;
            public string author;
            public string version_number;
            public string website_url;
            public string description;
            public string[] dependencies;
        }
#pragma warning restore 0649

        //[MenuItem(Constants.ThunderStorePath + nameof(ThunderstoreManifest), true, priority = Core.Constants.ThunderKitMenuPriority)]
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
                    var stubManifest = JsonUtility.FromJson<ThunderstoreManifestStub>(json);
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


        //[MenuItem(Constants.ThunderStorePath + nameof(ThunderstoreManifest), false, priority = Core.Constants.ThunderKitMenuPriority)]
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
                    var stubManifest = JsonUtility.FromJson<ThunderstoreManifestStub>(json);
                    name = stubManifest.name;
                }

                SelectNewAsset<ThunderstoreManifest>(() => name);
                var instance = Selection.activeObject as ThunderstoreManifest;

                JsonUtility.FromJsonOverwrite(json, instance);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static ThunderstoreManifestStub LoadStub(string path) => JsonUtility.FromJson<ThunderstoreManifestStub>(File.ReadAllText(path));

    }
}
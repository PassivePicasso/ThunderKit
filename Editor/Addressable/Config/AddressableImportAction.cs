using System;
using System.IO;
using ThunderKit.Common.Configuration;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Addressable.Config
{
    using static ThunderKit.Common.PathExtensions;
    public class AddressableImportAction : ConfigureAction
    {
        public override string Name => "Import Addressable Catalog";

        public override void Execute()
        {
            if (ImportAddressableData(ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>()))
            {
                EditorApplication.update += UpdateDefines;
            }
        }

        private static void UpdateDefines()
        {
            if (EditorApplication.isUpdating) return;
            EditorApplication.update -= UpdateDefines;
            ScriptingSymbolManager.AddScriptingDefine("TK_ADDRESSABLE");
        }

        private static bool ImportAddressableData(ThunderKitSettings settings)
        {
            if (!File.Exists(settings.AddressableAssetsCatalog)) return false;
            if (!File.Exists(settings.AddressableAssetsSettings)) return false;

            try
            {
                string destinationFolder = Combine("Assets", "StreamingAssets", "aa");
                Directory.CreateDirectory(destinationFolder);

                var destinationCatalog = Combine(destinationFolder, "catalog.json");
                var destinationSettings = Combine(destinationFolder, "settings.json");
                if (File.Exists(destinationCatalog)) File.Delete(destinationCatalog);
                if (File.Exists(destinationSettings)) File.Delete(destinationSettings);

                //var catalog = File.ReadAllText(settings.AddressableAssetsCatalog);
                //catalog = catalog.Replace(AddressableRuntimePath, ThunderKitRuntimePath);
                //File.WriteAllText(destinationCatalog, catalog);

                File.Copy(settings.AddressableAssetsCatalog, destinationCatalog);
                File.Copy(settings.AddressableAssetsSettings, destinationSettings);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
    }
}
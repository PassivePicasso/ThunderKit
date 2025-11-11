using System;
using System.IO;
using ThunderKit.Common;
using ThunderKit.Common.Configuration;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using UnityEngine;

namespace ThunderKit.Addressable.Config
{
    using static ThunderKit.Common.PathExtensions;
    public class ImportAddressableCatalog : OptionalExecutor
    {
        public override int Priority => ThunderKit.Common.Constants.Priority.AddressableCatalog;

        public override bool Execute()
        {
            if (ImportAddressableData(ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>()))
            {
                ScriptingSymbolManager.AddScriptingDefine("TK_ADDRESSABLE");
            }
            return true;
        }

        private static bool ImportAddressableData(ThunderKitSettings settings)
        {
            if (!File.Exists(settings.AddressableAssetsCatalog)
            && !File.Exists(settings.AddressableAssetsCatalog.Replace("json", "bin"))) return false;
            if (!File.Exists(settings.AddressableAssetsSettings)) return false;

            try
            {
                var catalogFullPath = settings.AddressableAssetsCatalog;
                var isBin = File.Exists(catalogFullPath.Replace("json", "bin"));
                var catalogFileName = isBin ? "catalog.bin" : "catalog.json";
                var catalogHash = "catalog.hash";
                catalogFullPath = isBin ? catalogFullPath.Replace("json", "bin") : catalogFullPath;

                string destinationFolder = Combine("Assets", "StreamingAssets", "aa");
                Directory.CreateDirectory(destinationFolder);

                var destinationCatalog = Combine(destinationFolder, catalogFileName);
                if (File.Exists(destinationCatalog)) File.Delete(destinationCatalog);
                File.Copy(catalogFullPath, destinationCatalog);

                if (isBin)
                {
                    var catalogHashFullPath = Path.Combine(Path.GetDirectoryName(catalogFullPath), catalogHash);
                    var destinationCatalogHash = Combine(destinationFolder, catalogHash);
                    if (File.Exists(destinationCatalogHash)) File.Delete(destinationCatalogHash);
                    File.Copy(catalogHashFullPath, destinationCatalogHash);
                }

                var destinationSettings = Combine(destinationFolder, "settings.json");
                if (File.Exists(destinationSettings)) File.Delete(destinationSettings);
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

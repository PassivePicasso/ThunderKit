using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Linq;
using System.Net;
using ThunderKit.Common;
using UnityEngine;

namespace ThunderKit.Core.Utilities
{
    internal static class ClassDataManager
    {
        const string TpkDownloadUrl =
            "https://nightly.link/AssetRipper/Tpk/workflows/type_tree_tpk/master/uncompressed_file.zip";

        static readonly string CacheDir = Path.Combine("Library", "ThunderKit");
        static readonly string CachedTpkPath = Path.Combine("Library", "ThunderKit", "classdata.tpk");
        static readonly string MetadataPath = Path.Combine("Library", "ThunderKit", "classdata.tpk.json");
        static readonly TimeSpan StalenessThreshold = TimeSpan.FromDays(7);

        [Serializable]
        class TpkMetadata
        {
            public string downloadedUtc;
        }

        public static string GetClassDataPath()
        {
            if (File.Exists(CachedTpkPath) && !IsCacheStale())
                return CachedTpkPath;

            if (TryDownloadTpk())
                return CachedTpkPath;

            Debug.LogWarning("[ThunderKit] Using bundled classdata.tpk as fallback");
            return Constants.BundledClassDataPath;
        }

        static bool IsCacheStale()
        {
            if (!File.Exists(MetadataPath))
                return true;

            try
            {
                var json = File.ReadAllText(MetadataPath);
                var metadata = JsonUtility.FromJson<TpkMetadata>(json);
                var downloadedUtc = DateTime.Parse(metadata.downloadedUtc).ToUniversalTime();
                return (DateTime.UtcNow - downloadedUtc) > StalenessThreshold;
            }
            catch
            {
                return true;
            }
        }

        static bool TryDownloadTpk()
        {
            try
            {
                Debug.LogWarning("[ThunderKit] Downloading tpk archive");
                Directory.CreateDirectory(CacheDir);
                Directory.CreateDirectory(Constants.TempDir);

                var tempZipPath = Path.Combine(Constants.TempDir, "classdata_download.zip");

                using (var client = new WebClient())
                {
                    client.DownloadFile(TpkDownloadUrl, tempZipPath);
                }

                using (var archive = ArchiveFactory.Open(tempZipPath))
                {
                    var tpkEntry = archive.Entries
                        .FirstOrDefault(e => !e.IsDirectory && e.Key.EndsWith(".tpk", StringComparison.OrdinalIgnoreCase));

                    if (tpkEntry == null)
                    {
                        Debug.LogWarning("[ThunderKit] Downloaded archive does not contain a .tpk file");
                        return false;
                    }

                    tpkEntry.WriteToDirectory(CacheDir, new ExtractionOptions
                    {
                        ExtractFullPath = false,
                        Overwrite = true
                    });

                    // The extracted file may have a different name (e.g. "uncompressed.tpk")
                    var extractedName = Path.GetFileName(tpkEntry.Key);
                    var extractedPath = Path.Combine(CacheDir, extractedName);
                    if (!string.Equals(extractedName, "classdata.tpk", StringComparison.OrdinalIgnoreCase)
                        && File.Exists(extractedPath))
                    {
                        if (File.Exists(CachedTpkPath))
                            File.Delete(CachedTpkPath);
                        File.Move(extractedPath, CachedTpkPath);
                    }
                }

                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);

                WriteMetadata();
                Debug.Log("[ThunderKit] Successfully downloaded updated classdata.tpk");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ThunderKit] Failed to download updated classdata.tpk: {e.Message}");
                return false;
            }
        }

        static void WriteMetadata()
        {
            var metadata = new TpkMetadata
            {
                downloadedUtc = DateTime.UtcNow.ToString("o")
            };
            File.WriteAllText(MetadataPath, JsonUtility.ToJson(metadata));
        }
    }
}

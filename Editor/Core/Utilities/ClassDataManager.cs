using AssetsTools.NET.Extra;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.Globalization;
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
        // Marker recording the last time a download attempt failed to yield support
        // for the current Unity version. Throttles re-downloads (see RetryThrottle).
        static readonly string MetadataPath = Path.Combine("Library", "ThunderKit", "classdata.tpk.json");
        static readonly TimeSpan RetryThrottle = TimeSpan.FromDays(1);

        internal enum ClassDataStatus
        {
            CacheSupported,
            DownloadedSupported,
            Throttled,
            UnsupportedAfterDownload,
            DownloadFailed,
        }

        [Serializable]
        class TpkMetadata
        {
            public string lastAttemptUtc;
        }

        public static string GetClassDataPath()
        {
            var unityVersion = Application.unityVersion;
            var cacheSupports = SupportsVersion(CachedTpkPath, unityVersion);
            var throttled = IsThrottledNow(DateTime.UtcNow);

            var status = PlanAcquisition(
                cacheSupports,
                throttled,
                tryDownload: TryDownloadTpk,
                cacheSupportsAfterDownload: () => SupportsVersion(CachedTpkPath, unityVersion));

            switch (status)
            {
                case ClassDataStatus.CacheSupported:
                case ClassDataStatus.DownloadedSupported:
                    ClearAttemptMarker();
                    return CachedTpkPath;

                case ClassDataStatus.UnsupportedAfterDownload:
                    SafeDelete(CachedTpkPath);
                    WriteAttemptMarker(DateTime.UtcNow);
                    ReportUnsupported(unityVersion);
                    return null;

                case ClassDataStatus.Throttled:
                    ReportUnsupported(unityVersion);
                    return null;

                case ClassDataStatus.DownloadFailed:
                    WriteAttemptMarker(DateTime.UtcNow);
                    Debug.LogError($"[ThunderKit] Could not download class data and no cached classdata.tpk supports Unity {unityVersion}.");
                    return null;

                default:
                    return null;
            }
        }

        internal static ClassDataStatus PlanAcquisition(bool cacheSupports, bool throttled,
            Func<bool> tryDownload, Func<bool> cacheSupportsAfterDownload)
        {
            if (cacheSupports)
                return ClassDataStatus.CacheSupported;

            if (throttled)
                return ClassDataStatus.Throttled;

            if (!tryDownload())
                return ClassDataStatus.DownloadFailed;

            return cacheSupportsAfterDownload()
                ? ClassDataStatus.DownloadedSupported
                : ClassDataStatus.UnsupportedAfterDownload;
        }

        internal static bool SupportsVersion(string tpkPath, string unityVersion)
        {
            if (!File.Exists(tpkPath))
                return false;
            if (!TryParseUnityVersion(unityVersion, out var major, out var minor, out var patch))
                return false;

            try
            {
                var manager = new AssetsManager();
                var package = manager.LoadClassPackage(tpkPath);
                var versions = package?.TpkTypeTree?.Versions;
                if (versions == null)
                    return false;

                return versions.Any(v => v.major == major && v.minor == minor && v.patch == patch);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ThunderKit] Failed to inspect classdata.tpk versions: {e.Message}");
                return false;
            }
        }

        internal static bool TryParseUnityVersion(string unityVersion, out int major, out int minor, out int patch)
        {
            major = minor = patch = 0;
            if (string.IsNullOrEmpty(unityVersion))
                return false;

            var parts = unityVersion.Split('.');
            if (parts.Length < 3)
                return false;

            if (!int.TryParse(parts[0], out major))
                return false;
            if (!int.TryParse(parts[1], out minor))
                return false;

            var patchDigits = new string(parts[2].TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(patchDigits, out patch);
        }

        static bool IsThrottledNow(DateTime nowUtc)
        {
            if (!File.Exists(MetadataPath))
                return false;

            try
            {
                if (!TryReadLastAttemptUtc(File.ReadAllText(MetadataPath), out var lastAttemptUtc))
                    return false;

                return IsThrottled(lastAttemptUtc, nowUtc, RetryThrottle);
            }
            catch
            {
                return false;
            }
        }

        internal static bool IsThrottled(DateTime lastAttemptUtc, DateTime nowUtc, TimeSpan window)
        {
            return (nowUtc - lastAttemptUtc) < window;
        }

        internal static bool TryReadLastAttemptUtc(string json, out DateTime lastAttemptUtc)
        {
            lastAttemptUtc = default;
            try
            {
                var metadata = JsonUtility.FromJson<TpkMetadata>(json);
                if (metadata == null || string.IsNullOrEmpty(metadata.lastAttemptUtc))
                    return false;

                lastAttemptUtc = DateTime.Parse(metadata.lastAttemptUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind).ToUniversalTime();
                return true;
            }
            catch
            {
                return false;
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

                if (ExtractTpkFromArchive(tempZipPath, CacheDir, CachedTpkPath) == null)
                {
                    Debug.LogWarning("[ThunderKit] Downloaded archive does not contain a .tpk file");
                    return false;
                }

                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);

                Debug.Log("[ThunderKit] Successfully downloaded updated classdata.tpk");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ThunderKit] Failed to download updated classdata.tpk: {e.Message}");
                return false;
            }
        }

        internal static string ExtractTpkFromArchive(string archivePath, string destDir, string finalTpkPath)
        {
            using (var archive = ArchiveFactory.Open(archivePath))
            {
                var tpkEntry = archive.Entries
                    .FirstOrDefault(e => !e.IsDirectory && e.Key.EndsWith(".tpk", StringComparison.OrdinalIgnoreCase));

                if (tpkEntry == null)
                    return null;

                tpkEntry.WriteToDirectory(destDir, new ExtractionOptions
                {
                    ExtractFullPath = false,
                    Overwrite = true
                });

                // The extracted file may have a different name (e.g. "uncompressed.tpk")
                var extractedName = Path.GetFileName(tpkEntry.Key);
                var extractedPath = Path.Combine(destDir, extractedName);
                var finalName = Path.GetFileName(finalTpkPath);
                if (!string.Equals(extractedName, finalName, StringComparison.OrdinalIgnoreCase)
                    && File.Exists(extractedPath))
                {
                    if (File.Exists(finalTpkPath))
                        File.Delete(finalTpkPath);
                    File.Move(extractedPath, finalTpkPath);
                }

                return finalTpkPath;
            }
        }

        static void ReportUnsupported(string unityVersion)
        {
            Debug.LogError($"[ThunderKit] The available class data (tpk) does not yet support the current version of Unity ({unityVersion}). " +
                "ProjectSettings import that relies on class data will be unavailable until AssetRipper publishes type data for this version.");
        }

        static void WriteAttemptMarker(DateTime nowUtc)
        {
            try
            {
                Directory.CreateDirectory(CacheDir);
                var metadata = new TpkMetadata { lastAttemptUtc = nowUtc.ToString("o") };
                File.WriteAllText(MetadataPath, JsonUtility.ToJson(metadata));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ThunderKit] Failed to write class data attempt marker: {e.Message}");
            }
        }

        static void ClearAttemptMarker()
        {
            SafeDelete(MetadataPath);
        }

        static void SafeDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ThunderKit] Failed to delete {path}: {e.Message}");
            }
        }
    }
}

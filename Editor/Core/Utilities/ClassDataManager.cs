using AssetsTools.NET.Extra;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
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
            "https://nightly.link/AssetRipper/Tpk/workflows/type_tree_tpk/master/lz4_file.zip";

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
                case ClassDataStatus.DownloadFailed:
                    WriteAttemptMarker(DateTime.UtcNow);
                    return BestAvailableOrNull(unityVersion);

                case ClassDataStatus.Throttled:
                    return BestAvailableOrNull(unityVersion);

                default:
                    return null;
            }
        }

        // Returns the cached tpk (with a warning that it may not fully cover the running
        // Unity version) when one exists, or null with an error when none is available.
        static string BestAvailableOrNull(string unityVersion)
        {
            if (File.Exists(CachedTpkPath))
            {
                WarnVersionNotCovered(unityVersion);
                return CachedTpkPath;
            }

            Debug.LogError($"[ThunderKit] No classdata.tpk is available and one could not be downloaded for Unity {unityVersion}. ProjectSettings import will be skipped.");
            return null;
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
            // Coverage is decided on major.minor only. Type trees rarely change in a
            // patch release, and our use (ProjectSettings) touches a small, stable set
            // of types, so requiring an exact patch match would reject usable tpks.
            if (!TryParseUnityVersion(unityVersion, out var major, out var minor, out _))
                return false;

            try
            {
                var manager = new AssetsManager();
                var package = manager.LoadClassPackage(tpkPath);
                var versions = package?.TpkTypeTree?.Versions;
                if (versions == null)
                    return false;

                return versions.Any(v => v.major == major && v.minor == minor);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ThunderKit] Failed to inspect classdata.tpk versions: {e.Message}");
                return false;
            }
        }

        // Picks the tpk version to build a class database from for the running Unity
        // version. Type trees are additive, so the correct choice is the newest entry
        // at or before the target; if the target predates every entry, fall back to the
        // oldest available so we still produce a best-effort database rather than an
        // empty one. Returns null only when no versions are available.
        internal static UnityVersion SelectBestVersion(List<UnityVersion> versions, int major, int minor, int patch)
        {
            if (versions == null || versions.Count == 0)
                return null;

            var target = VersionKey(major, minor, patch);

            UnityVersion bestAtOrBelow = null;
            var bestAtOrBelowKey = long.MinValue;
            UnityVersion oldest = null;
            var oldestKey = long.MaxValue;

            foreach (var v in versions)
            {
                if (v == null)
                    continue;

                var key = VersionKey(v.major, v.minor, v.patch);
                if (key <= target && key > bestAtOrBelowKey)
                {
                    bestAtOrBelowKey = key;
                    bestAtOrBelow = v;
                }
                if (key < oldestKey)
                {
                    oldestKey = key;
                    oldest = v;
                }
            }

            return bestAtOrBelow ?? oldest;
        }

        static long VersionKey(int major, int minor, int patch)
        {
            return ((long)major << 40) | ((long)minor << 20) | (uint)patch;
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

        static void WarnVersionNotCovered(string unityVersion)
        {
            Debug.LogWarning($"[ThunderKit] The available class data (tpk) does not list Unity {unityVersion}. " +
                "ProjectSettings import will proceed using the closest available type data. " +
                "Individual settings may fail to import if their type information is unavailable for this version; " +
                "such failures will be reported per-setting.");
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

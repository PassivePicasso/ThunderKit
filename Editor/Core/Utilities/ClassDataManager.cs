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

        // How a tpk stands relative to the bundled AssetsTools.NET and a Unity version.
        internal enum TpkState
        {
            Missing,
            // Container format newer than the bundled AssetsTools.NET, or corrupt.
            // Unusable regardless of which Unity versions it covers.
            Unreadable,
            Uncovered,
            Covered,
        }

        internal enum TpkDownloadResult
        {
            Failed,
            // Fetched, but unreadable here. Discarded instead of promoted.
            Incompatible,
            Downloaded,
        }

        internal enum ClassDataStatus
        {
            CacheSupported,
            DownloadedSupported,
            Throttled,
            UnsupportedAfterDownload,
            DownloadIncompatible,
            DownloadFailed,
        }

        internal enum ClassDataResolution
        {
            UseCache,
            UseCacheWithWarning,
            None,
        }

        [Serializable]
        class TpkMetadata
        {
            public string lastAttemptUtc;
        }

        public static string GetClassDataPath()
        {
            var unityVersion = Application.unityVersion;
            var cacheState = InspectTpk(CachedTpkPath, unityVersion);

            var status = PlanAcquisition(
                cacheState,
                IsThrottledNow(DateTime.UtcNow),
                tryDownload: () => TryDownloadTpk(unityVersion),
                cacheStateAfterDownload: () => InspectTpk(CachedTpkPath, unityVersion));

            switch (status)
            {
                case ClassDataStatus.CacheSupported:
                case ClassDataStatus.DownloadedSupported:
                    ClearAttemptMarker();
                    return CachedTpkPath;

                case ClassDataStatus.DownloadIncompatible:
                    WarnTpkFormatUnsupported();
                    WriteAttemptMarker(DateTime.UtcNow);
                    return PathFor(cacheState, unityVersion);

                case ClassDataStatus.UnsupportedAfterDownload:
                    WriteAttemptMarker(DateTime.UtcNow);
                    return PathFor(InspectTpk(CachedTpkPath, unityVersion), unityVersion);

                case ClassDataStatus.DownloadFailed:
                    WriteAttemptMarker(DateTime.UtcNow);
                    return PathFor(cacheState, unityVersion);

                case ClassDataStatus.Throttled:
                    return PathFor(cacheState, unityVersion);

                default:
                    return null;
            }
        }

        internal static ClassDataStatus PlanAcquisition(TpkState cacheState, bool throttled,
            Func<TpkDownloadResult> tryDownload, Func<TpkState> cacheStateAfterDownload)
        {
            if (cacheState == TpkState.Covered)
                return ClassDataStatus.CacheSupported;

            if (throttled)
                return ClassDataStatus.Throttled;

            switch (tryDownload())
            {
                case TpkDownloadResult.Failed:
                    return ClassDataStatus.DownloadFailed;

                case TpkDownloadResult.Incompatible:
                    return ClassDataStatus.DownloadIncompatible;
            }

            return cacheStateAfterDownload() == TpkState.Covered
                ? ClassDataStatus.DownloadedSupported
                : ClassDataStatus.UnsupportedAfterDownload;
        }

        // An uncovered tpk is still worth using — SelectBestVersion falls back to the
        // closest type data — but an unreadable one can only throw at load time.
        internal static ClassDataResolution ResolveFromState(TpkState state)
        {
            switch (state)
            {
                case TpkState.Covered:
                    return ClassDataResolution.UseCache;

                case TpkState.Uncovered:
                    return ClassDataResolution.UseCacheWithWarning;

                default:
                    return ClassDataResolution.None;
            }
        }

        static string PathFor(TpkState state, string unityVersion)
        {
            switch (ResolveFromState(state))
            {
                case ClassDataResolution.UseCache:
                    return CachedTpkPath;

                case ClassDataResolution.UseCacheWithWarning:
                    WarnVersionNotCovered(unityVersion);
                    return CachedTpkPath;

                default:
                    Debug.LogError($"[ThunderKit] No usable class data (classdata.tpk) is available for Unity {unityVersion}. ProjectSettings import will be skipped.");
                    return null;
            }
        }

        internal static TpkState InspectTpk(string tpkPath, string unityVersion)
        {
            if (!File.Exists(tpkPath))
                return TpkState.Missing;

            List<UnityVersion> versions;
            try
            {
                versions = new AssetsManager().LoadClassPackage(tpkPath)?.TpkTypeTree?.Versions;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ThunderKit] Class data at {tpkPath} could not be read by the AssetsTools.NET bundled with this version of ThunderKit: {e.Message}");
                return TpkState.Unreadable;
            }

            if (versions == null)
                return TpkState.Unreadable;

            // Coverage is decided on major.minor only. Type trees rarely change in a
            // patch release, and our use (ProjectSettings) touches a small, stable set
            // of types, so requiring an exact patch match would reject usable tpks.
            if (!TryParseUnityVersion(unityVersion, out var major, out var minor, out _))
                return TpkState.Uncovered;

            return versions.Any(v => v.major == major && v.minor == minor)
                ? TpkState.Covered
                : TpkState.Uncovered;
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

        // Downloads into a staging directory and promotes over the cache only after the
        // bundled AssetsTools.NET proves it can read the result, so a tpk published in a
        // newer container format cannot destroy a cache that still works.
        static TpkDownloadResult TryDownloadTpk(string unityVersion)
        {
            var stagingDir = Path.Combine(Constants.TempDir, "classdata_staging");
            try
            {
                Debug.Log("[ThunderKit] Downloading tpk archive");
                Directory.CreateDirectory(CacheDir);
                Directory.CreateDirectory(Constants.TempDir);
                SafeDeleteDirectory(stagingDir);
                Directory.CreateDirectory(stagingDir);

                var tempZipPath = Path.Combine(Constants.TempDir, "classdata_download.zip");

                using (var client = new WebClient())
                {
                    client.DownloadFile(TpkDownloadUrl, tempZipPath);
                }

                var stagedTpkPath = ExtractTpkFromArchive(
                    tempZipPath, stagingDir, Path.Combine(stagingDir, "classdata.tpk"));
                SafeDelete(tempZipPath);

                if (stagedTpkPath == null)
                {
                    Debug.LogWarning("[ThunderKit] Downloaded archive does not contain a .tpk file");
                    return TpkDownloadResult.Failed;
                }

                if (InspectTpk(stagedTpkPath, unityVersion) == TpkState.Unreadable)
                    return TpkDownloadResult.Incompatible;

                PromoteToCache(stagedTpkPath);
                Debug.Log("[ThunderKit] Successfully downloaded updated classdata.tpk");
                return TpkDownloadResult.Downloaded;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ThunderKit] Failed to download updated classdata.tpk: {e.Message}");
                return TpkDownloadResult.Failed;
            }
            finally
            {
                SafeDeleteDirectory(stagingDir);
            }
        }

        // Copies alongside the cache and lands it with a same-directory move, so a
        // failure part way through leaves the old tpk intact rather than truncated.
        static void PromoteToCache(string stagedTpkPath)
        {
            var pendingPath = CachedTpkPath + ".new";
            File.Copy(stagedTpkPath, pendingPath, true);

            if (File.Exists(CachedTpkPath))
                File.Delete(CachedTpkPath);

            File.Move(pendingPath, CachedTpkPath);
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

        static void WarnTpkFormatUnsupported()
        {
            Debug.LogWarning("[ThunderKit] The downloaded class data (classdata.tpk) uses a container format newer " +
                "than the AssetsTools.NET bundled with this version of ThunderKit. The download was discarded and any " +
                "previously cached class data is kept. Update ThunderKit to pick up support for the new format.");
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

        static void SafeDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ThunderKit] Failed to delete {path}: {e.Message}");
            }
        }
    }
}

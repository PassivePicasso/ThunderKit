using System;
using System.Collections.Generic;
using AssetsTools.NET.Extra;
using NUnit.Framework;
using ThunderKit.Core.Utilities;

namespace ThunderKitTests
{
    // Tier A — TPK acquisition policy. Pure, deterministic; no network or game data.
    // Exercises the version-coverage decision, re-download throttle, version parsing,
    // and marker parsing of ClassDataManager (visible via InternalsVisibleTo on
    // ThunderKit.Core). The tpk is validated by whether it covers the running Unity
    // version, never by age.
    [TestFixture]
    public class ClassDataManagerTests
    {
        static readonly TimeSpan OneDay = TimeSpan.FromDays(1);
        static readonly DateTime Now = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);

        static Func<bool> Fail(string name) => () => throw new AssertionException($"{name} should not have been invoked");

        // --- PlanAcquisition: the full control-flow, effects injected ---

        [Test]
        public void Plan_CacheCoversVersion_UsesCacheWithoutDownloading()
        {
            var status = ClassDataManager.PlanAcquisition(
                cacheSupports: true,
                throttled: false,
                tryDownload: Fail("tryDownload"),
                cacheSupportsAfterDownload: Fail("cacheSupportsAfterDownload"));

            Assert.That(status, Is.EqualTo(ClassDataManager.ClassDataStatus.CacheSupported));
        }

        [Test]
        public void Plan_StaleButCoveringCache_StillUsesCache()
        {
            // Age is irrelevant: a cache that covers the version is always preferred,
            // even if a fresher tpk could be downloaded.
            var status = ClassDataManager.PlanAcquisition(
                cacheSupports: true,
                throttled: true,
                tryDownload: Fail("tryDownload"),
                cacheSupportsAfterDownload: Fail("cacheSupportsAfterDownload"));

            Assert.That(status, Is.EqualTo(ClassDataManager.ClassDataStatus.CacheSupported));
        }

        [Test]
        public void Plan_NoCoverage_Throttled_DoesNotDownload()
        {
            var status = ClassDataManager.PlanAcquisition(
                cacheSupports: false,
                throttled: true,
                tryDownload: Fail("tryDownload"),
                cacheSupportsAfterDownload: Fail("cacheSupportsAfterDownload"));

            Assert.That(status, Is.EqualTo(ClassDataManager.ClassDataStatus.Throttled));
        }

        [Test]
        public void Plan_NoCoverage_DownloadFails_ReportsDownloadFailed()
        {
            var status = ClassDataManager.PlanAcquisition(
                cacheSupports: false,
                throttled: false,
                tryDownload: () => false,
                cacheSupportsAfterDownload: Fail("cacheSupportsAfterDownload"));

            Assert.That(status, Is.EqualTo(ClassDataManager.ClassDataStatus.DownloadFailed));
        }

        [Test]
        public void Plan_NoCoverage_DownloadAddsSupport_UsesDownloaded()
        {
            var status = ClassDataManager.PlanAcquisition(
                cacheSupports: false,
                throttled: false,
                tryDownload: () => true,
                cacheSupportsAfterDownload: () => true);

            Assert.That(status, Is.EqualTo(ClassDataManager.ClassDataStatus.DownloadedSupported));
        }

        [Test]
        public void Plan_NoCoverage_DownloadStillUnsupported_ReportsUnsupported()
        {
            var status = ClassDataManager.PlanAcquisition(
                cacheSupports: false,
                throttled: false,
                tryDownload: () => true,
                cacheSupportsAfterDownload: () => false);

            Assert.That(status, Is.EqualTo(ClassDataManager.ClassDataStatus.UnsupportedAfterDownload));
        }

        // --- Re-download throttle ---

        [Test]
        public void IsThrottled_WithinWindow_True()
        {
            Assert.That(ClassDataManager.IsThrottled(Now.AddHours(-1), Now, OneDay), Is.True);
        }

        [Test]
        public void IsThrottled_BeyondWindow_False()
        {
            Assert.That(ClassDataManager.IsThrottled(Now.AddDays(-2), Now, OneDay), Is.False);
        }

        [Test]
        public void IsThrottled_ExactlyAtWindow_False()
        {
            // Boundary: the check is strictly less-than, so exactly one window later
            // a retry is allowed again.
            Assert.That(ClassDataManager.IsThrottled(Now.AddDays(-1), Now, OneDay), Is.False);
        }

        // --- Unity version parsing ---

        [Test]
        [TestCase("6000.0.42f1", 6000, 0, 42)]
        [TestCase("2019.4.40f1", 2019, 4, 40)]
        [TestCase("2020.3.48f1b3", 2020, 3, 48)]
        [TestCase("2021.2.0b7", 2021, 2, 0)]
        public void TryParseUnityVersion_ValidStrings_ParseMajorMinorPatch(string version, int major, int minor, int patch)
        {
            Assert.That(ClassDataManager.TryParseUnityVersion(version, out var m, out var n, out var p), Is.True);
            Assert.That(new[] { m, n, p }, Is.EqualTo(new[] { major, minor, patch }));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("6000.0")]
        [TestCase("not.a.version")]
        public void TryParseUnityVersion_InvalidStrings_ReturnFalse(string version)
        {
            Assert.That(ClassDataManager.TryParseUnityVersion(version, out _, out _, out _), Is.False);
        }

        // --- Closest-version selection within the tpk ---

        static List<UnityVersion> Versions(params string[] versions)
        {
            var list = new List<UnityVersion>();
            foreach (var v in versions)
                list.Add(new UnityVersion(v));
            return list;
        }

        static string Format(UnityVersion v) => v == null ? "<null>" : $"{v.major}.{v.minor}.{v.patch}";

        [Test]
        public void SelectBestVersion_ExactMatch_ReturnsExact()
        {
            var best = ClassDataManager.SelectBestVersion(
                Versions("2019.4.40f1", "2021.3.16f1", "6000.0.42f1"), 2021, 3, 16);

            Assert.That(Format(best), Is.EqualTo("2021.3.16"));
        }

        [Test]
        public void SelectBestVersion_NoExact_PrefersNewestAtOrBelow()
        {
            // 2021.3.5 is not present; the additive type tree means the schema as of the
            // newest entry at or before it (2021.3.0) is the correct choice.
            var best = ClassDataManager.SelectBestVersion(
                Versions("2021.3.0f1", "2021.3.16f1"), 2021, 3, 5);

            Assert.That(Format(best), Is.EqualTo("2021.3.0"));
        }

        [Test]
        public void SelectBestVersion_TargetNewerThanAll_ReturnsNewestAvailable()
        {
            var best = ClassDataManager.SelectBestVersion(
                Versions("2019.4.40f1", "2021.3.16f1"), 6000, 0, 42);

            Assert.That(Format(best), Is.EqualTo("2021.3.16"));
        }

        [Test]
        public void SelectBestVersion_TargetOlderThanAll_FallsBackToOldest()
        {
            var best = ClassDataManager.SelectBestVersion(
                Versions("2021.3.16f1", "6000.0.42f1"), 2018, 4, 0);

            Assert.That(Format(best), Is.EqualTo("2021.3.16"));
        }

        [Test]
        public void SelectBestVersion_EmptyOrNull_ReturnsNull()
        {
            Assert.That(ClassDataManager.SelectBestVersion(new List<UnityVersion>(), 2021, 3, 16), Is.Null);
            Assert.That(ClassDataManager.SelectBestVersion(null, 2021, 3, 16), Is.Null);
        }

        // --- Attempt-marker parsing ---

        [Test]
        public void TryReadLastAttemptUtc_ValidJson_ParsesTimestamp()
        {
            var json = "{\"lastAttemptUtc\":\"2024-01-03T04:05:06.0000000Z\"}";

            Assert.That(ClassDataManager.TryReadLastAttemptUtc(json, out var dt), Is.True);
            Assert.That(dt.ToUniversalTime(), Is.EqualTo(new DateTime(2024, 1, 3, 4, 5, 6, DateTimeKind.Utc)));
        }

        [Test]
        public void TryReadLastAttemptUtc_MissingField_ReturnsFalse()
        {
            Assert.That(ClassDataManager.TryReadLastAttemptUtc("{}", out _), Is.False);
        }

        [Test]
        public void TryReadLastAttemptUtc_Garbage_ReturnsFalse()
        {
            Assert.That(ClassDataManager.TryReadLastAttemptUtc("not json at all", out _), Is.False);
        }
    }
}

using NUnit.Framework;
using System.IO;
using ThunderKit.Core.Utilities;
using UnityEngine;

namespace ThunderKitTests
{
    // Tier B — TPK version coverage (integration). Marked [Explicit] because it
    // reaches the network (downloads the AssetRipper tpk) and is not deterministic
    // offline. Run on demand from the Unity Test Runner to verify the acquired tpk
    // actually contains a class database for the Unity version under test. In CI the
    // per-version test matrix is what provides multi-version coverage.
    [TestFixture]
    public class ClassDataVersionCoverageTests
    {
        [Test]
        [Explicit("Downloads the AssetRipper tpk over the network; run manually.")]
        [Category("Integration")]
        public void AcquiredTpk_CoversRunningUnityVersion()
        {
            var tpkPath = ClassDataManager.GetClassDataPath();

            Assert.That(tpkPath, Is.Not.Null.And.Not.Empty,
                $"No classdata.tpk could be obtained for Unity {Application.unityVersion} (download failed or version unsupported).");
            Assert.That(File.Exists(tpkPath), Is.True, $"Resolved tpk path does not exist: {tpkPath}");
            Assert.That(ClassDataManager.SupportsVersion(tpkPath, Application.unityVersion), Is.True,
                $"Acquired classdata.tpk does not contain a class database for Unity {Application.unityVersion}.");
        }
    }
}

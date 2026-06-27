using NUnit.Framework;
using System.IO;
using ThunderKit.Core.Utilities;
using UnityEngine;

namespace ThunderKitTests
{
    // Tier B — TPK version coverage (integration). Reaches the network (downloads
    // the AssetRipper tpk) and is not deterministic offline, so it is tagged
    // [Category("Integration")] and excluded from CI via -testCategory "!Integration"
    // (see .github/workflows/main.yml). It runs by default in the local Unity Test
    // Runner, where it verifies the acquired tpk actually contains a class database
    // for the Unity version under test.
    [TestFixture]
    public class ClassDataVersionCoverageTests
    {
        [Test]
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

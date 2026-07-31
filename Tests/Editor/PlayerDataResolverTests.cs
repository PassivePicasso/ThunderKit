using System.IO;
using AssetsTools.NET.Extra;
using NUnit.Framework;
using ThunderKit.Core.Utilities;

namespace ThunderKitTests
{
    // Tier A — pure, offline, no AssetDatabase or fixtures.
    //
    // Covers the two player layouts PlayerDataResolver has to span: serialized files loose
    // in <Game>_Data, and the same files packed into a data.unity3d bundle by a player
    // built with chunk based compression. The loose layout's end to end export is covered
    // by ImportProjectSettingsTests; here the bundle branch is driven against a synthesized
    // UnityFS bundle, which reaches entry lookup and header parsing but not asset loading.
    [TestFixture]
    public class PlayerDataResolverTests
    {
        const string BundleEngineVersion = "2019.4.28f1";

        string workingDirectory;

        [SetUp]
        public void SetUp()
        {
            workingDirectory = Path.Combine(
                Directory.GetCurrentDirectory(), "Temp", "PlayerDataResolverTests", TestContext.CurrentContext.Test.ID);
            Directory.CreateDirectory(workingDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(workingDirectory))
                Directory.Delete(workingDirectory, true);
        }

        [Test]
        public void TryGetPlayerDataPath_PrefersLooseGlobalGameManagers()
        {
            WriteFile(PlayerDataResolver.GlobalGameManagers, new byte[] { 0 });
            WriteFile(PlayerDataResolver.CompressedPlayerData, new byte[] { 0 });

            Assert.That(PlayerDataResolver.TryGetPlayerDataPath(workingDirectory, out var playerDataPath), Is.True);
            Assert.That(Path.GetFileName(playerDataPath), Is.EqualTo(PlayerDataResolver.GlobalGameManagers));
        }

        [Test]
        public void TryGetPlayerDataPath_FallsBackToCompressedPlayerData()
        {
            WriteFile(PlayerDataResolver.CompressedPlayerData, new byte[] { 0 });

            Assert.That(PlayerDataResolver.TryGetPlayerDataPath(workingDirectory, out var playerDataPath), Is.True);
            Assert.That(Path.GetFileName(playerDataPath), Is.EqualTo(PlayerDataResolver.CompressedPlayerData));
        }

        [Test]
        public void TryGetPlayerDataPath_ReturnsFalseWhenNeitherLayoutIsPresent()
        {
            Assert.That(PlayerDataResolver.TryGetPlayerDataPath(workingDirectory, out var playerDataPath), Is.False);
            Assert.That(playerDataPath, Is.Null);
        }

        [Test]
        public void TryGetPlayerDataPath_ReturnsFalseForEmptyGameDataPath()
        {
            Assert.That(PlayerDataResolver.TryGetPlayerDataPath(null, out _), Is.False);
            Assert.That(PlayerDataResolver.TryGetPlayerDataPath(string.Empty, out _), Is.False);
        }

        [Test]
        public void TryGetPlayerUnityVersion_ReadsVersionFromCompressedPlayerHeader()
        {
            var bundlePath = WriteBundle(PlayerDataResolver.GlobalGameManagers);

            Assert.That(PlayerDataResolver.TryGetPlayerUnityVersion(bundlePath, out var unityVersion), Is.True);
            Assert.That(unityVersion, Is.EqualTo(BundleEngineVersion));
        }

        [Test]
        public void TryGetPlayerUnityVersion_ReturnsFalseForUnreadableFile()
        {
            var path = WriteFile("garbage.bin", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

            Assert.That(PlayerDataResolver.TryGetPlayerUnityVersion(path, out var unityVersion), Is.False);
            Assert.That(unityVersion, Is.Null);
        }

        [Test]
        public void TryGetPlayerUnityVersion_ReturnsFalseForMissingFile()
        {
            var missing = Path.Combine(workingDirectory, PlayerDataResolver.CompressedPlayerData);

            Assert.That(PlayerDataResolver.TryGetPlayerUnityVersion(missing, out _), Is.False);
        }

        // The entry lookup is the version sensitive part of the bundle branch: a compressed
        // player whose root serialized file is spelled differently must report what it did
        // contain rather than failing anonymously.
        [Test]
        public void LoadGlobalGameManagers_ReportsBundleEntriesWhenGlobalGameManagersIsAbsent()
        {
            var bundlePath = WriteBundle("level0", "sharedassets0.assets");
            var assetsManager = new AssetsManager();

            try
            {
                var exception = Assert.Throws<PlayerDataResolver.MissingGlobalGameManagersException>(
                    () => PlayerDataResolver.LoadGlobalGameManagers(assetsManager, bundlePath));

                Assert.That(exception.Message, Does.Contain("level0"));
                Assert.That(exception.Message, Does.Contain("sharedassets0.assets"));
            }
            finally
            {
                assetsManager.UnloadAll(true);
            }
        }

        [Test]
        public void LoadGlobalGameManagers_FindsEntryRegardlessOfCase()
        {
            var bundlePath = WriteBundle("level0", "GlobalGameManagers");
            var assetsManager = new AssetsManager();

            try
            {
                // The synthesized entry is filler rather than a serialized file, so reaching
                // the load failure is what proves the lookup matched it.
                var exception = Assert.Throws<PlayerDataResolver.MissingGlobalGameManagersException>(
                    () => PlayerDataResolver.LoadGlobalGameManagers(assetsManager, bundlePath));

                Assert.That(exception.Message, Does.Contain("not a readable serialized file"));
            }
            finally
            {
                assetsManager.UnloadAll(true);
            }
        }

        [Test]
        public void LoadGlobalGameManagers_ThrowsForMissingPlayerData()
        {
            var missing = Path.Combine(workingDirectory, PlayerDataResolver.GlobalGameManagers);

            Assert.Throws<FileNotFoundException>(
                () => PlayerDataResolver.LoadGlobalGameManagers(new AssetsManager(), missing));
        }

        string WriteFile(string fileName, byte[] contents)
        {
            var path = Path.Combine(workingDirectory, fileName);
            File.WriteAllBytes(path, contents);
            return path;
        }

        string WriteBundle(params string[] entryNames)
        {
            return WriteFile(
                PlayerDataResolver.CompressedPlayerData,
                UnityFsBundleBuilder.Build(BundleEngineVersion, entryNames));
        }
    }
}

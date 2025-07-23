namespace ThunderKitTests
{
    using NUnit.Framework;
    using ThunderKit.Core.Data;
    using ThunderKit.Core.Windows;
    using System.Linq;

    [TestFixture]
    public class PackageSourceSettingsTests
    {
        private PackageSourceSettingsFixture sourceSettings;

        [SetUp]
        public void SetUp()
        {
            sourceSettings = new PackageSourceSettingsFixture();
        }

        [TearDown]
        public void TearDown()
        {
            sourceSettings.Dispose();
        }

        [Test]
        public void AddSource_ThunderStore()
        {
            sourceSettings.AddThunderStoreSource();

            int numSourcesDisplayed = sourceSettings.GetListedSources().Count;
            int numSourcesRegistered = PackageSourceSettings.PackageSources.Count;
            Assert.That(numSourcesDisplayed, Is.EqualTo(sourceSettings.NumSourcesInitially + 1));
            Assert.That(numSourcesRegistered, Is.EqualTo(sourceSettings.NumSourcesInitially + 1));
        }

        [Test]
        public void RemoveSource()
        {
            var addedSource = sourceSettings.AddThunderStoreSource();

            // Select the added source, and press remove
            sourceSettings.RemoveSource(addedSource);

            // List of sources should be same as initially
            Assert.That(sourceSettings.GetListedSources(), Is.EquivalentTo(sourceSettings.InitialPackageSources));
            Assert.That(PackageSourceSettings.PackageSources, Is.EquivalentTo(sourceSettings.InitialPackageSources));
        }
    }

    [TestFixture]
    public class PackageManagerTests
    {
        private PackageSourceSettingsFixture sourceSettings;
        private PackageManagerFixture packageManager;

        [SetUp]
        public void SetUp()
        {
            sourceSettings = new PackageSourceSettingsFixture();
            packageManager = new PackageManagerFixture();
        }

        [TearDown]
        public void TearDown()
        {
            sourceSettings.Dispose();
            packageManager.Dispose();
        }


        private void AssertMenusListSameSources()
        {
            var listedSources_packageSourceSettings = sourceSettings.GetListedSources();
            var listedSources_packageManager = packageManager.GetListedSources();
            Assert.That(listedSources_packageSourceSettings, Is.EquivalentTo(listedSources_packageManager),
                "PackageManager and PackageSourceSettings do not list the same package sources.");
        }

        [Test, Description("PackageManager lists same sources as PackageSourceSettings")]
        public void PackageManagerMatchesSourceSettings()
        {
            Assert.IsNotNull(packageManager.PackageSourceList, "Could not find #tkpm-package-source-list");
            AssertMenusListSameSources();
        }

        [Test, Description("Add a Source in settings, and check that it shows up in Package Manager")]
        public void Refresh_AddSource()
        {
            sourceSettings.AddThunderStoreSource();
            sourceSettings.Settings.Refresh();
            Assert.That(packageManager.GetListedSources().Count, Is.EqualTo(sourceSettings.NumSourcesInitially + 1));
            AssertMenusListSameSources();
        }

        [Test, Description("Remove a Source in settings, and check that it disappears from Package Manager")]
        public void Refresh_RemoveSource()
        {
            // Setup, add a source and make sure package manager lists it
            var addedSource = sourceSettings.AddThunderStoreSource();
            sourceSettings.Settings.Refresh();

            // Remove this source, refresh, and make sure package manager does not list it anymore
            sourceSettings.RemoveSource(addedSource);
            sourceSettings.Settings.Refresh();

            // Verify both lists are same as initially
            AssertMenusListSameSources();
            Assert.That(sourceSettings.GetListedSources(), Is.EquivalentTo(sourceSettings.InitialPackageSources));
            Assert.That(PackageSourceSettings.PackageSources, Is.EquivalentTo(sourceSettings.InitialPackageSources));
            Assert.That(packageManager.GetListedSources(), Is.EquivalentTo(sourceSettings.InitialPackageSources));
        }
    }
}
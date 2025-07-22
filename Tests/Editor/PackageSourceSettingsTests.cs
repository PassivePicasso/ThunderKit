namespace ThunderKitTests
{
    using NUnit.Framework;
    using ThunderKit.Core.Data;
    using UnityEngine;
    using UnityEngine.UIElements;
    using ThunderKit.Core.Windows;
    using System.Linq;

    [TestFixture]
    public class PackageSourceSettingsTests
    {
        private void AssertMenusListSameSources(PackageManagerFixture packageManager, PackageSourceSettingsFixture sourceSettings)
        {
            var listedSources_packageSourceSettings = sourceSettings.GetListedSources();
            var listedSources_packageManager = packageManager.GetListedSources();
            Assert.That(listedSources_packageSourceSettings, Is.EquivalentTo(listedSources_packageManager),
                "PackageManager and PackageSourceSettings do not list the same package sources.");
        }

        [Test]
        public void AddSource_ThunderStore()
        {
            using var sourceSettings = new PackageSourceSettingsFixture();
            sourceSettings.AddThunderStoreSource();

            int numSourcesDisplayed = sourceSettings.GetListedSources().Count;
            int numSourcesRegistered = PackageSourceSettings.PackageSources.Count;
            Assert.That(numSourcesDisplayed, Is.EqualTo(sourceSettings.NumSourcesInitially + 1));
            Assert.That(numSourcesRegistered, Is.EqualTo(sourceSettings.NumSourcesInitially + 1));
        }

        [Test]
        public void RemoveSource()
        {
            using var sourceSettings = new PackageSourceSettingsFixture();
            var addedSource = sourceSettings.AddThunderStoreSource();

            // Select the added source, and press remove
            sourceSettings.RemoveSource(addedSource);

            // List of sources should be same as initially
            Assert.That(sourceSettings.GetListedSources(), Is.EquivalentTo(sourceSettings.InitialPackageSources));
            Assert.That(PackageSourceSettings.PackageSources, Is.EquivalentTo(sourceSettings.InitialPackageSources));
        }

        [Test, Description("PackageManager lists same sources as PackageSourceSettings")]
        public void PackageManagerMatchesSourceSettings()
        {
            using var sourceSettings = new PackageSourceSettingsFixture();
            using var packageManager = new PackageManagerFixture();
            Assert.IsNotNull(packageManager.PackageSourceList, "Could not find #tkpm-package-source-list");

            AssertMenusListSameSources(packageManager, sourceSettings);
        }

        [Test]
        public void Refresh_AddSource()
        {
            using var sourceSettings = new PackageSourceSettingsFixture();
            using var packageManager = new PackageManagerFixture();

            sourceSettings.AddThunderStoreSource();
            sourceSettings.Settings.Refresh();
            Assert.That(packageManager.GetListedSources().Count, Is.EqualTo(sourceSettings.NumSourcesInitially + 1));
            AssertMenusListSameSources(packageManager, sourceSettings);
        }

        [Test]
        public void Refresh_RemoveSource()
        {
            // Setup, add a source and make sure package manager lists it
            using var sourceSettings = new PackageSourceSettingsFixture();
            using var packageManager = new PackageManagerFixture();
            var addedSource = sourceSettings.AddThunderStoreSource();
            sourceSettings.Settings.Refresh();

            // Remove this source, refresh, and make sure package manager does not list it anymore
            sourceSettings.RemoveSource(addedSource);
            sourceSettings.Settings.Refresh();

            // Verify both lists are same as initially
            AssertMenusListSameSources(packageManager, sourceSettings);
            Assert.That(sourceSettings.GetListedSources(), Is.EquivalentTo(sourceSettings.InitialPackageSources));
            Assert.That(PackageSourceSettings.PackageSources, Is.EquivalentTo(sourceSettings.InitialPackageSources));
            Assert.That(packageManager.GetListedSources(), Is.EquivalentTo(sourceSettings.InitialPackageSources));
        }
    }
}
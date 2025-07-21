namespace ThunderKitTests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using ThunderKit.Core.Data;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [TestFixture]
    public class PackageSourceSettingsTests
    {
        private List<PackageSource> initialPackageSources;
        private VisualElement root;
        private PackageSourceSettings packageSourceSettings;
        private ListView sourcesList;
        private int numSourcesInitially;

        [SetUp]
        public void Init()
        {
            initialPackageSources = PackageSourceSettings.PackageSources.ToList();

            root = new VisualElement();
            packageSourceSettings = ScriptableObject.CreateInstance<PackageSourceSettings>();
            packageSourceSettings.CreateSettingsUI(root);
            sourcesList = root.Q<ListView>("sources-list");

            numSourcesInitially = sourcesList.itemsSource.Count;
        }

        [TearDown]
        public void Cleanup()
        {
            // Delete all sources that were added during the test
            var introducedSources = PackageSourceSettings.PackageSources
                                        .Except(initialPackageSources)
                                        .ToList();
            foreach (var source in introducedSources)
            {
                PackageSourceSettings.RemoveSource(source);
            }
            EditorApplication.update();
            Assert.That(PackageSourceSettings.PackageSources, Is.EqualTo(initialPackageSources));
        }

        private PackageSource Utility_AddThunderStoreSource()
        {
            var priorPackageSources = PackageSourceSettings.PackageSources.ToList();

            // Press Add->ThunderStoreSource
            Type[] packageSourceTypes = PackageSourceSettings.GetAvailablePackageSourceTypes();
            var thunderstoreSourceType = packageSourceTypes.First(x => x.Name == "ThunderstoreSource");
            packageSourceSettings.AddSource(thunderstoreSourceType);
            EditorApplication.update();

            return PackageSourceSettings.PackageSources
                .Except(priorPackageSources)
                .FirstOrDefault(); // return the newly added source
        }

        [Test]
        public void AddSource_ThunderStore()
        {
            Utility_AddThunderStoreSource();
            Assert.That(sourcesList.itemsSource.Count, Is.EqualTo(numSourcesInitially + 1));
            Assert.That(PackageSourceSettings.PackageSources.Count, Is.EqualTo(numSourcesInitially + 1));
        }

        [Test]
        public void RemoveSource()
        {
            var addedSource = Utility_AddThunderStoreSource();
            
            // Select the added source, and press remove
            sourcesList.selectedIndex = sourcesList.itemsSource.IndexOf(addedSource);
            packageSourceSettings.RemoveSourceClicked();
            EditorApplication.update();

            // List of sources should be same as initially
            Assert.That(sourcesList.itemsSource.Count, Is.EqualTo(numSourcesInitially));
            Assert.That(PackageSourceSettings.PackageSources.Count, Is.EqualTo(numSourcesInitially));
            Assert.That(PackageSourceSettings.PackageSources, Is.EqualTo(initialPackageSources));
        }
    }
}
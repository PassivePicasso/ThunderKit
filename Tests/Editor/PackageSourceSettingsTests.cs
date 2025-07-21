namespace ThunderKitTests
{
    using Boo.Lang.Runtime;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using ThunderKit.Core.Data;
    using UnityEditor;
    using UnityEngine;
#if UNITY_2019_1_OR_NEWER
    using UnityEngine.UIElements;
    using UnityEditor.UIElements;
#else
    using UnityEngine.Experimental.UIElements;
#endif

    [TestFixture]
    public class SanityTests
    {
        [SetUp]
        public void Init()
        {
            Debug.Log("testing");
        }

        [TearDown]
        public void Cleanup()
        {
            Debug.Log("cleaning up");
        }

        [Test]
        public void SanityTest()
        {
            Assert.AreEqual(1, 1);
        }
    }

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
            packageSourceSettings = ScriptableObject.CreateInstance<PackageSourceSettings>();
            root = new VisualElement();
            packageSourceSettings.CreateSettingsUI(root);
            sourcesList = root.Q<ListView>("sources-list");

            Utility_EditorApplicationUpdate();
            initialPackageSources = PackageSourceSettings.PackageSources.ToList();
            numSourcesInitially = sourcesList.itemsSource.Count;
        }

        [Test]
        public void SanityTest()
        {
            Assert.AreEqual(1, 1);
        }

        [TearDown]
        public void Cleanup()
        {
            // Delete all sources that were added during the test
            Utility_EditorApplicationUpdate();
            var introducedSources = PackageSourceSettings.PackageSources
                                        .Except(initialPackageSources)
                                        .ToList();
            foreach (var source in introducedSources)
            {
                PackageSourceSettings.RemoveSource(source);
            }
            Utility_EditorApplicationUpdate();
            Assert.That(PackageSourceSettings.PackageSources, Is.EqualTo(initialPackageSources));
        }

        private void Utility_EditorApplicationUpdate()
        {
#if UNITY_2019_1_OR_NEWER
            EditorApplication.update();
#else
            // In Unity 2018 and prior, EditorApplication is paused during tests,
            // so EditorApplication.update() does not run. Invoke methods directly to refresh the package sources.
            var type = typeof(PackageSourceSettings);
            var methodDeferredRefresh = type.GetMethod("DeferredRefresh", BindingFlags.NonPublic | BindingFlags.Static);
            var methodRefreshList = type.GetMethod("RefreshList", BindingFlags.NonPublic | BindingFlags.Instance);
            methodDeferredRefresh.Invoke(null, null);
            methodRefreshList.Invoke(packageSourceSettings, null);
#endif
        }

        private PackageSource Utility_AddThunderStoreSource()
        {
            var priorPackageSources = PackageSourceSettings.PackageSources.ToList();

            // Press Add->ThunderStoreSource
            Type[] packageSourceTypes = PackageSourceSettings.GetAvailablePackageSourceTypes();
            var thunderstoreSourceType = packageSourceTypes.First(x => x.Name == "ThunderstoreSource");
            packageSourceSettings.AddSource(thunderstoreSourceType);
            Utility_EditorApplicationUpdate();

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
            Utility_EditorApplicationUpdate();

            // List of sources should be same as initially
            Assert.That(sourcesList.itemsSource.Count, Is.EqualTo(numSourcesInitially));
            Assert.That(PackageSourceSettings.PackageSources.Count, Is.EqualTo(numSourcesInitially));
            Assert.That(PackageSourceSettings.PackageSources, Is.EqualTo(initialPackageSources));
        }
    }
}
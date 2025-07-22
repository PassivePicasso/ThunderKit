using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThunderKit.Core.Data;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
    using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKitTests
{

    internal class PackageSourceSettingsFixture : IDisposable
    {
        public VisualElement Root { get; private set; }
        public PackageSourceSettings Settings { get; private set; }
        public ListView SourcesList { get; private set; }
        public readonly List<PackageSource> InitialPackageSources;
        public int NumSourcesInitially => InitialPackageSources.Count;

        public PackageSourceSettingsFixture()
        {
            Root = new VisualElement();
            Settings = ScriptableObject.CreateInstance<PackageSourceSettings>();
            Settings.CreateSettingsUI(Root);
            SourcesList = Root.Q<ListView>("sources-list");

            EditorApplicationUpdate();
            InitialPackageSources = PackageSourceSettings.PackageSources.ToList();
        }

        public void Dispose()
        {
            // Delete all sources that were added during the test
            EditorApplicationUpdate();
            var introducedSources = PackageSourceSettings.PackageSources
                                        .Except(InitialPackageSources)
                                        .ToList();
            foreach (var source in introducedSources)
            {
                PackageSourceSettings.RemoveSource(source);
            }
            EditorApplicationUpdate();

            if (!PackageSourceSettings.PackageSources.SequenceEqual(InitialPackageSources))
            {
                throw new InvalidOperationException(
                    "Test did not clean up PackageSources correctly, PackageSources now differ from before test was ran.");
            }
        }

        public List<PackageSource> GetListedSources()
        {
            return SourcesList.itemsSource.Cast<PackageSource>().ToList();
        }

        public void EditorApplicationUpdate()
        {
#if UNITY_2021_0_OR_NEWER
            EditorApplication.update();
#else
            // In Unity 2021 and prior, EditorApplication is paused during tests,
            // so EditorApplication.update() does not run. Invoke methods directly to refresh the package sources.
            var type = typeof(PackageSourceSettings);
            var methodDeferredRefresh = type.GetMethod("DeferredRefresh", BindingFlags.NonPublic | BindingFlags.Static);
            var methodRefreshList = type.GetMethod("RefreshList", BindingFlags.NonPublic | BindingFlags.Instance);
            methodDeferredRefresh.Invoke(null, null);
            methodRefreshList.Invoke(Settings, null);
#endif
        }

        public PackageSource AddThunderStoreSource()
        {
            var priorPackageSources = PackageSourceSettings.PackageSources.ToList();

            // Press Add->ThunderStoreSource
            Type[] packageSourceTypes = PackageSourceSettings.GetAvailablePackageSourceTypes();
            var thunderstoreSourceType = packageSourceTypes.First(x => x.Name == "ThunderstoreSource");
            Settings.AddSource(thunderstoreSourceType);
            EditorApplicationUpdate();

            return PackageSourceSettings.PackageSources
                .Except(priorPackageSources)
                .FirstOrDefault(); // return the newly added source
        }

        /// <summary>
        /// Selects the given source in the SourcesList and clicks the Remove button.
        /// </summary>
        public void RemoveSource(PackageSource source)
        {
            int indexOfAddedSource = SourcesList.itemsSource.IndexOf(source);
            SourcesList.selectedIndex = indexOfAddedSource;
            Settings.RemoveSourceClicked();
            EditorApplicationUpdate();
        }
    }
}
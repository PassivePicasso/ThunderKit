using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;

namespace ThunderKitTests
{
    // ImportConfiguration holds its executors as sub-assets of itself and keeps a
    // separate list of references to them. The list is only useful if it survives a
    // domain reload, so these tests read the saved bytes back rather than trusting
    // the live instance, which can hold references that were never written.
    [TestFixture]
    public class ImportConfigurationTests
    {
        const string AssetPath = "Assets/__TK_ImportConfigurationTest__.asset";
        const string ReloadedPath = "Assets/__TK_ImportConfigurationTest_Reloaded__.asset";

        ImportConfiguration configuration;

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(AssetPath); // clean any leftover from a crashed run
            AssetDatabase.DeleteAsset(ReloadedPath);
            configuration = ScriptableObject.CreateInstance<ImportConfiguration>();
            AssetDatabase.CreateAsset(configuration, AssetPath);
            configuration.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(AssetPath);
            AssetDatabase.DeleteAsset(ReloadedPath);
        }

        [Test]
        public void ConfigurationExecutors_SurviveReload()
        {
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetPath)
                .OfType<OptionalExecutor>()
                .ToArray();
            Assert.That(subAssets.Length, Is.GreaterThan(0), "Initialize created no executor sub-assets");

            var reloaded = LoadSavedCopy();

            Assert.That(reloaded.ConfigurationExecutors, Is.Not.Null);
            Assert.That(reloaded.ConfigurationExecutors.Length, Is.EqualTo(subAssets.Length),
                "Executor references were not saved alongside the executors themselves");
            foreach (var executor in reloaded.ConfigurationExecutors)
                Assert.That(executor != null, Is.True, "Saved configuration holds a broken executor reference");
        }

        [Test]
        public void ConfigurationExecutors_OrderIsIndependentOfSubAssetOrder()
        {
            var executors = configuration.ConfigurationExecutors;
            var deterministic = executors
                .OrderByDescending(executor => executor.Priority)
                .ThenBy(executor => executor.GetType().FullName, StringComparer.Ordinal)
                .ToArray();

            Assert.That(executors, Is.EqualTo(deterministic),
                "Equal-priority executors are ordered by sub-asset order, which varies by editor version");

            var reloaded = LoadSavedCopy().ConfigurationExecutors.Select(executor => executor.GetType().FullName);
            Assert.That(reloaded, Is.EqualTo(executors.Select(executor => executor.GetType().FullName)),
                "Saved executor order differs from the order that produced it");
        }

        [Test]
        public void RelinkExecutors_UnchangedList_DoesNotRewrite()
        {
            var before = configuration.ConfigurationExecutors;

            InvokeRelinkExecutors(configuration);

            Assert.That(configuration.ConfigurationExecutors, Is.SameAs(before),
                "Relinking rewrote an unchanged list, dirtying the asset on every domain reload");
        }

        // Copying forces a load from the saved bytes, which is what a restarted editor
        // sees. Reimporting in place can refresh the live instance instead, hiding a
        // list that was never written.
        ImportConfiguration LoadSavedCopy()
        {
            AssetDatabase.SaveAssets();
            Assert.That(AssetDatabase.CopyAsset(AssetPath, ReloadedPath), Is.True, $"Could not copy {AssetPath}");

            var copy = AssetDatabase.LoadAssetAtPath<ImportConfiguration>(ReloadedPath);
            Assert.That(copy, Is.Not.Null, $"Copied configuration at {ReloadedPath} did not load");
            return copy;
        }

        static void InvokeRelinkExecutors(ImportConfiguration target)
        {
            var relink = typeof(ImportConfiguration)
                .GetMethod("RelinkExecutors", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(relink, Is.Not.Null, "ImportConfiguration.RelinkExecutors was renamed or removed");
            relink.Invoke(target, null);
        }
    }
}

using NUnit.Framework;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Manifests.Datum;
using ThunderKit.Core.Paths.Components;
using ThunderKit.Core.Pipelines;
using UnityEngine;

namespace ThunderKitTests
{
    [TestFixture]
    public class ManifestNameTests
    {
        // With no per-manifest context (ManifestIndex defaults to 0 and Manifests is
        // null), ManifestName falls back to the pipeline's root `manifest` field.
        [Test]
        public void GetPath_NoManifestContext_UsesRootManifestIdentityName()
        {
            var pipeline = ScriptableObject.CreateInstance<Pipeline>();
            var manifest = ScriptableObject.CreateInstance<Manifest>();
            manifest.Identity = ScriptableObject.CreateInstance<ManifestIdentity>();
            manifest.Identity.Name = "MyMod";
            pipeline.manifest = manifest;

            var manifestName = ScriptableObject.CreateInstance<ManifestName>();

            Assert.That(manifestName.GetPath(null, pipeline), Is.EqualTo("MyMod"));

            Object.DestroyImmediate(manifestName);
            Object.DestroyImmediate(manifest.Identity);
            Object.DestroyImmediate(manifest);
            Object.DestroyImmediate(pipeline);
        }

        [Test]
        public void GetPath_NoManifestAssigned_ReturnsNull()
        {
            var pipeline = ScriptableObject.CreateInstance<Pipeline>();
            var manifestName = ScriptableObject.CreateInstance<ManifestName>();

            // targetManifest is null, so the null-conditional chain returns null
            // without entering the catch block.
            Assert.That(manifestName.GetPath(null, pipeline), Is.Null);

            Object.DestroyImmediate(manifestName);
            Object.DestroyImmediate(pipeline);
        }
    }
}

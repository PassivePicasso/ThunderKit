using System;
using NUnit.Framework;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Manifests.Datum;
using ThunderKit.Integrations.Thunderstore;
using ThunderKit.Integrations.Thunderstore.Jobs;
using UnityEngine;
using static ThunderKit.Integrations.Thunderstore.CreateThunderstoreManifest;

namespace ThunderKitTests
{
    // RenderJson is a public instance method on a PipelineJob (ScriptableObject),
    // so it can be exercised directly without running a pipeline.
    [TestFixture]
    public class StageThunderstoreManifestTests
    {
        static ManifestIdentity Identity(string author, string name, string version, string description = null)
        {
            var identity = ScriptableObject.CreateInstance<ManifestIdentity>();
            identity.Author = author;
            identity.Name = name;
            identity.Version = version;
            identity.Description = description;
            identity.Dependencies = new Manifest[0];
            return identity;
        }

        [Test]
        public void RenderJson_NoDependencies_RoundTripsIdentityFields()
        {
            var identity = Identity("Author", "MyMod", "1.2.3", "A test mod");
            var data = ScriptableObject.CreateInstance<ThunderstoreData>();
            data.url = "https://example.com";
            var job = ScriptableObject.CreateInstance<StageThunderstoreManifest>();

            var json = job.RenderJson(identity, data);
            var stub = JsonUtility.FromJson<ThunderstoreManifestStub>(json);

            Assert.That(stub.author, Is.EqualTo("Author"));
            Assert.That(stub.name, Is.EqualTo("MyMod"));
            Assert.That(stub.version_number, Is.EqualTo("1.2.3"));
            Assert.That(stub.description, Is.EqualTo("A test mod"));
            Assert.That(stub.website_url, Is.EqualTo("https://example.com"));
            Assert.That(stub.dependencies?.Length ?? 0, Is.EqualTo(0));
        }

        [Test]
        public void RenderJson_WithDependency_FormatsAuthorNameVersion()
        {
            var dep = ScriptableObject.CreateInstance<Manifest>();
            dep.Identity = ScriptableObject.CreateInstance<ManifestIdentity>();
            dep.Identity.Author = "DepAuthor";
            dep.Identity.Name = "DepMod";
            dep.Identity.Version = "2.0.0";

            var identity = Identity("Author", "MyMod", "1.0.0");
            identity.Dependencies = new[] { dep };
            var data = ScriptableObject.CreateInstance<ThunderstoreData>();
            var job = ScriptableObject.CreateInstance<StageThunderstoreManifest>();

            var json = job.RenderJson(identity, data);
            var stub = JsonUtility.FromJson<ThunderstoreManifestStub>(json);

            Assert.That(stub.dependencies, Is.EqualTo(new[] { "DepAuthor-DepMod-2.0.0" }));
        }

        [Test]
        public void RenderJson_DependencyMissingIdentity_Throws()
        {
            var dep = ScriptableObject.CreateInstance<Manifest>(); // no Identity assigned
            var identity = Identity("Author", "MyMod", "1.0.0");
            identity.Dependencies = new[] { dep };
            var data = ScriptableObject.CreateInstance<ThunderstoreData>();
            var job = ScriptableObject.CreateInstance<StageThunderstoreManifest>();

            Assert.That(() => job.RenderJson(identity, data), Throws.InstanceOf<ArgumentException>());
        }
    }
}

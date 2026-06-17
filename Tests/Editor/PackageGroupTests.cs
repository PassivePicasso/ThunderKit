using NUnit.Framework;
using ThunderKit.Core.Data;

namespace ThunderKitTests
{
    [TestFixture]
    public class PackageGroupTests
    {
        static PackageGroup MakeGroup()
        {
            return new PackageGroup
            {
                Author = "RiskOfRain",
                PackageName = "Unstable Terrain",
                Description = "Dangerous package for multiplayer",
                DependencyId = "riskofrain-unstable-terrain",
                Tags = new[] { "map", "multiplayer" },
                Versions = new[]
                {
                    new PackageVersion { version = "2.0.0", dependencyId = "riskofrain-unstable-terrain-2.0.0" },
                    new PackageVersion { version = "1.0.0", dependencyId = "riskofrain-unstable-terrain-1.0.0" },
                }
            };
        }

        // HasString does a case-insensitive substring search across
        // Author, PackageName, Description, DependencyId, and Tags.
        [TestCase("RiskOfRain")]   // author, exact case
        [TestCase("riskofrain")]   // author, different case
        [TestCase("Unstable")]     // package name substring
        [TestCase("dangerous")]    // description, different case
        [TestCase("multiplayer")]  // tag (also in description)
        public void HasString_FindsMatchAcrossFields(string query)
        {
            Assert.That(MakeGroup().HasString(query), Is.True);
        }

        [Test]
        public void HasString_ReturnsFalse_WhenNotFound()
        {
            Assert.That(MakeGroup().HasString("nonexistent-xyz"), Is.False);
        }

        [Test]
        public void HasString_EmptyString_MatchesFirstField()
        {
            // CompareInfo.IndexOf(field, "") returns 0, so an empty query matches.
            Assert.That(MakeGroup().HasString(string.Empty), Is.True);
        }

        [Test]
        public void Indexer_Latest_ReturnsFirstVersion()
        {
            Assert.That(MakeGroup()["latest"].version, Is.EqualTo("2.0.0"));
        }

        [Test]
        public void Indexer_ExactVersion_ReturnsMatchingVersion()
        {
            Assert.That(MakeGroup()["1.0.0"].version, Is.EqualTo("1.0.0"));
        }

        [Test]
        public void Indexer_UnknownVersion_ReturnsNull()
        {
            Assert.That(MakeGroup()["9.9.9"], Is.Null);
        }

        [Test]
        public void Equals_IsBasedOnDependencyIdOnly()
        {
            var a = new PackageGroup { DependencyId = "x", PackageName = "A" };
            var b = new PackageGroup { DependencyId = "x", PackageName = "B-different" };
            Assert.That(a.Equals(b), Is.True);
            Assert.That(a == b, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentDependencyId_NotEqual()
        {
            var a = new PackageGroup { DependencyId = "x" };
            var b = new PackageGroup { DependencyId = "y" };
            Assert.That(a == b, Is.False);
            Assert.That(a != b, Is.True);
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using ThunderKit.Core.Data;

namespace ThunderKitTests
{
    [TestFixture]
    public class PackageVersionTests
    {
        [Test]
        public void Equals_IsBasedOnDependencyIdOnly()
        {
            // Version field differs, dependencyId matches -> considered equal.
            var a = new PackageVersion { dependencyId = "pkg-1.0.0", version = "1.0.0" };
            var b = new PackageVersion { dependencyId = "pkg-1.0.0", version = "9.9.9" };
            Assert.That(a.Equals(b), Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentDependencyId_NotEqual()
        {
            var a = new PackageVersion { dependencyId = "pkg-1.0.0" };
            var b = new PackageVersion { dependencyId = "pkg-2.0.0" };
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void HashSet_DeduplicatesByDependencyId()
        {
            var set = new HashSet<PackageVersion>
            {
                new PackageVersion { dependencyId = "pkg-1.0.0" },
                new PackageVersion { dependencyId = "pkg-1.0.0" },
            };
            Assert.That(set.Count, Is.EqualTo(1));
        }
    }
}

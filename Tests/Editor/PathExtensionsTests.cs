using NUnit.Framework;
using ThunderKit.Common;

namespace ThunderKitTests
{
    [TestFixture]
    public class PathExtensionsTests
    {
        [Test]
        public void Combine_JoinsPartsWithForwardSlashes()
        {
            // Path.Combine uses the platform separator; Combine normalizes it to '/'.
            Assert.That(PathExtensions.Combine("Packages", "com.example", "file.txt"),
                Is.EqualTo("Packages/com.example/file.txt"));
        }

        [Test]
        public void Combine_NormalizesBackslashesWithinParts()
        {
            Assert.That(PathExtensions.Combine("a\\b", "c"), Does.Not.Contain("\\"));
        }

        [Test]
        public void Combine_SinglePart_ReturnsPartUnchanged()
        {
            Assert.That(PathExtensions.Combine("Assets"), Is.EqualTo("Assets"));
        }
    }
}

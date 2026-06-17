using System;
using NUnit.Framework;
using ThunderKit.Core.Paths;

namespace ThunderKitTests
{
    // ResolvePath is static. With a null pipeline it falls back to scanning the
    // AssetDatabase for PathReferences (available in EditMode), so these tests
    // avoid depending on any specific project asset: one input has no token, and
    // the other uses a deliberately non-existent token name.
    [TestFixture]
    public class PathReferenceTests
    {
        [Test]
        public void ResolvePath_WithoutTokens_NormalizesBackslashes()
        {
            var result = PathReference.ResolvePath("some\\plain\\path", null, null);
            Assert.That(result, Is.EqualTo("some/plain/path"));
        }

        [Test]
        public void ResolvePath_UnknownToken_ThrowsInvalidOperationException()
        {
            Assert.That(
                () => PathReference.ResolvePath("<__NonExistentPathReference_Test__>", null, null),
                Throws.InstanceOf<InvalidOperationException>());
        }
    }
}

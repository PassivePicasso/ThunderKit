using NUnit.Framework;
using ThunderKit.Core.Data;

namespace ThunderKitTests
{
    // FileIdUtil lives in the global namespace in ThunderKit.Common; it derives a
    // stable Unity fileID from a type's namespace + name via an MD4 hash.
    [TestFixture]
    public class FileIdUtilTests
    {
        [Test]
        public void Compute_IsDeterministicForSameType()
        {
            Assert.That(FileIdUtil.Compute(typeof(PackageGroup)),
                Is.EqualTo(FileIdUtil.Compute(typeof(PackageGroup))));
        }

        [Test]
        public void Compute_ProducesDistinctIdsForDistinctTypes()
        {
            Assert.That(FileIdUtil.Compute(typeof(PackageGroup)),
                Is.Not.EqualTo(FileIdUtil.Compute(typeof(PackageVersion))));
        }
    }
}

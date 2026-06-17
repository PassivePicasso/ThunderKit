using System.ComponentModel;
using NUnit.Framework;
using ThunderKit.Common;

namespace ThunderKitTests
{
    [TestFixture]
    public class EnumExtensionsTests
    {
        enum SampleEnum
        {
            [Description("A friendly description")]
            Described,
            Undescribed
        }

        [Test]
        public void GetDescription_ReturnsDescriptionAttribute_WhenPresent()
        {
            Assert.That(SampleEnum.Described.GetDescription(), Is.EqualTo("A friendly description"));
        }

        [Test]
        public void GetDescription_FallsBackToMemberName_WhenNoAttribute()
        {
            Assert.That(SampleEnum.Undescribed.GetDescription(), Is.EqualTo("Undescribed"));
        }
    }
}

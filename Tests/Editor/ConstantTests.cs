using NUnit.Framework;
using ThunderKit.Core.Paths.Components;
using UnityEngine;

namespace ThunderKitTests
{
    [TestFixture]
    public class ConstantTests
    {
        [Test]
        public void GetPath_ReturnsConfiguredValue()
        {
            // Constant.GetPathInternal ignores the output/pipeline arguments and
            // returns its Value verbatim, so nulls are fine here.
            var constant = ScriptableObject.CreateInstance<Constant>();
            constant.Value = "MyConstantPath";

            Assert.That(constant.GetPath(null, null), Is.EqualTo("MyConstantPath"));

            Object.DestroyImmediate(constant);
        }
    }
}

using NUnit.Framework;
using ThunderKit.Markdown.Helpers;

namespace ThunderKitTests
{
    [TestFixture]
    public class UnityPathUtilityTests
    {
        // IsAssetDirectory is true only for paths under Assets or Packages,
        // with or without a leading slash.
        [TestCase("Assets/foo.png", true)]
        [TestCase("/Assets/foo.png", true)]
        [TestCase("Packages/com.x/foo.png", true)]
        [TestCase("/Packages/com.x/foo.png", true)]
        [TestCase("Library/cache", false)]
        [TestCase("Temp/x", false)]
        [TestCase("https://example.com/a.png", false)]
        public void IsAssetDirectory_IdentifiesProjectPaths(string path, bool expected)
        {
            Assert.That(UnityPathUtility.IsAssetDirectory(path), Is.EqualTo(expected));
        }
    }
}

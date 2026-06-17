using System;
using NUnit.Framework;
using ThunderKit.Core.Utilities;

namespace ThunderKitTests
{
    [TestFixture]
    public class PackageHelperTests
    {
        // GetCleanPackageName lowercases the input, returns it unchanged when it is
        // already a valid npm package name, and otherwise rewrites each character:
        //   '+' and '&' -> "and", alphanumerics/-/_/. kept, everything else -> '_'.
        [TestCase("my-valid-pkg", "my-valid-pkg")]   // already valid, untouched
        [TestCase("MyPackage", "mypackage")]         // valid after lowercasing
        [TestCase("My Package", "my_package")]       // space -> '_'
        [TestCase("MyPackage+Tool&Util", "mypackageandtoolandutil")] // '+' and '&' -> "and"
        [TestCase("!@#$%", "_____")]                 // each disallowed char -> '_'
        [TestCase("", "")]                           // empty stays empty
        public void GetCleanPackageName_SanitizesToNpmName(string input, string expected)
        {
            Assert.That(PackageHelper.GetCleanPackageName(input), Is.EqualTo(expected));
        }

        [Test]
        public void GetStringHashUTF8_IsDeterministic()
        {
            Assert.That(PackageHelper.GetStringHashUTF8("ThunderKit"),
                Is.EqualTo(PackageHelper.GetStringHashUTF8("ThunderKit")));
        }

        [Test]
        public void GetStringHashUTF8_DistinctInputsProduceDistinctHashes()
        {
            Assert.That(PackageHelper.GetStringHashUTF8("alpha"),
                Is.Not.EqualTo(PackageHelper.GetStringHashUTF8("beta")));
        }

        [Test]
        public void GetStringHashUTF8_IsAParseableGuid()
        {
            Assert.That(Guid.TryParse(PackageHelper.GetStringHashUTF8("ThunderKit"), out _), Is.True);
        }

        [Test]
        public void GetCleanedStringHashUTF8_Is32LowercaseHexWithoutDashes()
        {
            var cleaned = PackageHelper.GetCleanedStringHashUTF8("ThunderKit");
            Assert.That(cleaned, Has.Length.EqualTo(32));
            Assert.That(cleaned, Does.Not.Contain("-"));
            Assert.That(cleaned, Does.Match("^[0-9a-f]{32}$"));
        }

        [Test]
        public void GetCleanedStringHashUTF8_EqualsGuidHashWithoutFormatting()
        {
            // GetCleanedStringHashUTF8 is GetGuidHashUTF8 stringified, lowercased, dashes stripped.
            var expected = PackageHelper.GetGuidHashUTF8("ThunderKit").ToString().ToLower().Replace("-", "");
            Assert.That(PackageHelper.GetCleanedStringHashUTF8("ThunderKit"), Is.EqualTo(expected));
        }
    }
}

// SelfDestructingActionAsset.Action takes `int` on Unity < 6000.4 (EndNameEditAction)
// and `EntityId` on 6000.4+ (AssetCreationEndAction, see PR #116). EndNameEditAction is
// warning-obsolete on 6000.4 and error-level obsolete on 6000.5, so the production type
// switches to the EntityId signature at 6000.4 to keep the build warning-free. This test
// targets the int signature; the EntityId variant must be added once the tests are based
// on the 6000.4 compatibility changes from #116.
#if !UNITY_6000_4_OR_NEWER
using NUnit.Framework;
using ThunderKit.Core.Actions;
using UnityEngine;

namespace ThunderKitTests
{
    [TestFixture]
    public class SelfDestructingActionAssetTests
    {
        [Test]
        public void Action_InvokesDelegateWithArguments_ThenSelfDestructs()
        {
            var asset = ScriptableObject.CreateInstance<SelfDestructingActionAsset>();
            int capturedId = 0;
            string capturedPath = null;
            string capturedResource = null;
            asset.action = (id, path, resource) =>
            {
                capturedId = id;
                capturedPath = path;
                capturedResource = resource;
            };

            asset.Action(42, "Assets/New.asset", "resourceFile");

            Assert.That(capturedId, Is.EqualTo(42));
            Assert.That(capturedPath, Is.EqualTo("Assets/New.asset"));
            Assert.That(capturedResource, Is.EqualTo("resourceFile"));
            // CleanUp() runs DestroyImmediate(this); the Unity object compares equal to null.
            Assert.That(asset == null, Is.True);
        }
    }
}
#endif

// SelfDestructingActionAsset.Action takes `int` on Unity <= 6000.4 (EndNameEditAction)
// and `EntityId` on 6000.5+ (AssetCreationEndAction, see PR #116). EndNameEditAction is
// error-level obsolete on 6000.5, so the int-based type does not exist there. This test
// targets the int signature; the EntityId variant must be added once the tests are based
// on the 6000.5 compatibility changes from #116.
#if !UNITY_6000_5_OR_NEWER
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

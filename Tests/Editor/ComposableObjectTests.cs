using System;
using NUnit.Framework;
using ThunderKit.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ThunderKitTests
{
    // Minimal concrete types so the test is not coupled to a specific production
    // ComposableObject. TestComposable only supports TestComposableElement.
    internal class TestComposableElement : ComposableElement { }

    internal class TestComposable : ComposableObject
    {
        public override Type ElementType => typeof(TestComposableElement);
        public override bool SupportsType(Type type) => ElementType.IsAssignableFrom(type);
        public override string ElementTemplate => string.Empty;
    }

    [TestFixture]
    public class ComposableObjectTests
    {
        // InsertElement calls AssetDatabase.AddObjectToAsset(this), which requires
        // the ComposableObject to be a persisted asset, so each test gets a fresh
        // temp asset on disk.
        const string AssetPath = "Assets/__TK_ComposableObjectTest__.asset";
        TestComposable composable;

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(AssetPath); // clean any leftover from a crashed run
            composable = ScriptableObject.CreateInstance<TestComposable>();
            AssetDatabase.CreateAsset(composable, AssetPath);
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(AssetPath);
        }

        [Test]
        public void InsertElement_SupportedType_AddsToData()
        {
            var element = ScriptableObject.CreateInstance<TestComposableElement>();

            composable.InsertElement(element, 0);

            Assert.That(composable.Data, Is.Not.Null);
            Assert.That(composable.Data.Length, Is.EqualTo(1));
            Assert.That(composable.Data[0], Is.EqualTo(element));
        }

        [Test]
        public void InsertElement_UnsupportedType_IsNoOp()
        {
            // A bare ComposableElement is not assignable to TestComposableElement.
            var element = ScriptableObject.CreateInstance<ComposableElement>();

            composable.InsertElement(element, 0);

            Assert.That(composable.Data == null || composable.Data.Length == 0, Is.True);

            Object.DestroyImmediate(element);
        }

        [Test]
        public void RemoveElement_MatchingIndex_RemovesAndDestroysInstance()
        {
            var element = ScriptableObject.CreateInstance<TestComposableElement>();
            composable.InsertElement(element, 0);

            composable.RemoveElement(element, 0);

            Assert.That(composable.Data.Length, Is.EqualTo(0));
            Assert.That(element == null, Is.True); // DestroyImmediate'd by RemoveElement
        }

        [Test]
        public void RemoveElement_InstanceIndexMismatch_IsNoOpAndLogsError()
        {
            var element = ScriptableObject.CreateInstance<TestComposableElement>();
            composable.InsertElement(element, 0);
            var other = ScriptableObject.CreateInstance<TestComposableElement>();

            LogAssert.Expect(LogType.Error, "ComposableObject.RemoveElement: instance does not match index");
            composable.RemoveElement(other, 0);

            Assert.That(composable.Data.Length, Is.EqualTo(1));
            Assert.That(composable.Data[0], Is.EqualTo(element));

            Object.DestroyImmediate(other);
        }
    }
}

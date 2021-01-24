using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace PassivePicasso.ThunderKit.Core
{
    public abstract class ComposableObject : ScriptableObject
    {
        [FormerlySerializedAs("runSteps")]
        public ScriptableObject[] Data;

        public abstract bool SupportsType(Type type);

        public abstract Type ElementType { get; }

        public abstract string ElementTemplate { get; }
    }
}
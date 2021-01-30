using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThunderKit.Core
{
    public abstract class ComposableObject : ScriptableObject
    {
        [FormerlySerializedAs("runSteps")]
        public ComposableElement[] Data;

        public abstract bool SupportsType(Type type);

        public abstract Type ElementType { get; }

        public abstract string ElementTemplate { get; }
    }
}
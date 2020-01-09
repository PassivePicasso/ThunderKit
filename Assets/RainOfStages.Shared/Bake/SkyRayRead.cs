using RoR2.Navigation;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace RainOfStages.Bake
{
    public struct SkyRayRead : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<HitData> _hits;
        public NativeArray<Node> _nodes;

        public void Execute(Int32 index)
        {
            var node = _nodes[index];
            node.flags &= ~NodeFlags.NoCeiling;
            if (_hits[index].hit == 1) node.flags |= NodeFlags.NoCeiling;
            _nodes[index] = node;
        }
    }
}

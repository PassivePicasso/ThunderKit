using RoR2.Navigation;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

namespace RainOfStages.Bake
{
    [BurstCompile]
    public struct TPRayRead : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<HitData> _hits;
        public NativeArray<Node> _nodes;

        public Int32 offCount;

        public void Execute(Int32 index)
        {
            var off = index * offCount;
            bool ok = true;
            var node = _nodes[index];
            node.flags &= ~NodeFlags.TeleporterOK;

            for (Int32 i = 0; i < offCount; i++)
            {
                if (_hits[off + i].hit == 1)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) node.flags |= NodeFlags.TeleporterOK;

            _nodes[index] = node;
        }
    }
}

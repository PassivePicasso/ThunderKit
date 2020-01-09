using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

namespace RainOfStages.Bake
{
    [BurstCompile]
    public struct LOSRayGen : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Node> _nodes;
        [ReadOnly]
        public NativeArray<Link> _links;
        [WriteOnly]
        public NativeArray<RaycastCommand> _rays;

        public Int32 count;
        public Int32 mask;

        public void Execute(Int32 index)
        {
            var link = _links[index];

            if (link.valid == 0) return;

            var node1 = _nodes[link.node1ID];
            var node2 = _nodes[link.node2ID];

            float3 diff = node2.position - node1.position;

            float dist = math.sqrt(math.pow(diff.x, 2) + math.pow(diff.y, 2) + math.pow(diff.z, 2));

            _rays[index] = new RaycastCommand
            {
                from = node1.position + new float3(0, 1, 0),
                direction = diff / dist,
                distance = dist,
                layerMask = mask,
                maxHits = 1
            };
        }
    }
}

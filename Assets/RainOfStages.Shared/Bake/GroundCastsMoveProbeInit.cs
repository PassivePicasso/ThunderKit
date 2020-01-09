using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using RoR2;

namespace RainOfStages.Bake
{
    [BurstCompile]
    public struct GroundCastsMoveProbeInit : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Link> _links;
        [ReadOnly]
        public NativeArray<Node> _nodes;
        [WriteOnly]
        public NativeArray<RaycastCommand> _rays;

        [ReadOnly]
        public HullData hull;


        public void Execute(Int32 index)
        {
            var link = _links[index];
            if (link.valid == 0) return;
            var node1 = _nodes[link.node1ID];
            if (node1.forbiddenHulls == hull.hullMask) return;
            var node2 = _nodes[link.node2ID];
            if (node2.forbiddenHulls == hull.hullMask) return;
        }
    }
}

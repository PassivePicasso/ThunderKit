using RoR2;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace RainOfStages.Bake
{
    public struct SkyRayGen : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Node> _nodes;
        [WriteOnly]
        public NativeArray<RaycastCommand> _rays;
        public int layerMask;

        public void Execute(Int32 index)
        {
            _rays[index] = new RaycastCommand
            {
                from = _nodes[index].position,
                direction = new float3(0, 1, 0),
                distance = Single.PositiveInfinity,
                layerMask = layerMask,
                maxHits = 1
            };
        }
    }
}

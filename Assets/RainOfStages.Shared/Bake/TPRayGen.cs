using RoR2;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

namespace RainOfStages.Bake
{
    [BurstCompile]
    public struct TPRayGen : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Node> _nodes;
        [WriteOnly]
        public NativeArray<RaycastCommand> _rays;
        [ReadOnly]
        public NativeArray<float3> _offsets;
        public float range;
        public int layerMask;
        public void Execute(Int32 index)
        {
            var off = index * _offsets.Length;
            var orig = _nodes[index].position;
            for (Int32 i = 0; i < _offsets.Length; i++)
            {
                _rays[off + i] = new RaycastCommand
                {
                    from = orig + _offsets[i],
                    direction = new float3(0, -1, 0),
                    distance = range,
                    layerMask = layerMask,
                    maxHits = 1
                };
            }
        }
    }
}

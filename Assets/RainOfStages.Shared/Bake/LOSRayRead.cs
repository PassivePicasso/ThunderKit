using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

namespace RainOfStages.Bake
{
    [BurstCompile]
    public struct LOSRayRead : IJobParallelFor
    {
        public NativeArray<Link> _links;
        [ReadOnly]
        public NativeArray<HitData> _hitRes;

        public void Execute(Int32 index)
        {
            var hit = _hitRes[index];
            var link = _links[index];
            link.hasLOS = hit.hit;
            _links[index] = link;
        }
    }
}

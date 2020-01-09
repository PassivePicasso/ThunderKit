using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace RainOfStages.Bake
{
    [BurstCompile]
    public struct GenerateLinks : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<Link> Links;

        public int count;

        public void Execute(int index)
        {
            int y = Mathf.FloorToInt(index / count);
            int x = index % count;
            Boolean same = x == y;

            Links[index] = new Link
            {
                node1ID = x,
                node2ID = y,
                valid = x != y ? 1 : 0
            };
        }
    }
}

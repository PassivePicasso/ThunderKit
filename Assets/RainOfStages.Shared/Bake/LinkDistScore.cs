using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

namespace RainOfStages.Bake
{
    [BurstCompile]
    public struct LinkDistScore : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Node> _nodes;
        public NativeArray<Link> _links;

        public Single rangeFactor;

        public void Execute(Int32 index)
        {
            var link = _links[index];
            if (link.valid == 0) return;
            var pos1 = _nodes[link.node1ID];
            var pos2 = _nodes[link.node2ID];
            var vector = pos2.position - pos1.position;
            var sqrMag = math.pow(vector.x, 2) + math.pow(vector.y, 2) + math.pow(vector.z, 2);
            //float mag = (pos2.position - pos1.position).sqrMagnitude;

            if (sqrMag < rangeFactor)
            {
                link.distanceScore = Mathf.Sqrt(sqrMag);
            }
            else
            {
                link.valid = 0;
            }
            _links[index] = link;
        }
    }
}

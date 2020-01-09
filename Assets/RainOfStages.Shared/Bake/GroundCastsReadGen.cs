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
    public struct GroundCastsReadGen : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<HitData> _hitRes;
        [ReadOnly]
        public NativeArray<Node> _nodes;
        public NativeArray<Link> _links;
        public NativeArray<float3> _groundPos;

        public NativeArray<CapsulecastCommand> _caps;

        [ReadOnly]
        public HullData hull;
        [ReadOnly]
        public float3 fudge;
        public int layerMask;

        public void Execute(Int32 index)
        {
            var link = _links[index];
            if (link.valid == 0) return;
            var node1 = _nodes[link.node1ID];
            if (node1.forbiddenHulls == hull.hullMask) return;
            var node2 = _nodes[link.node2ID];
            if (node2.forbiddenHulls == hull.hullMask) return;

            var offset = index * 2;
            var hit1 = _hitRes[offset];
            var hit2 = _hitRes[offset + 1];

            Single radiusMod = hull.radius * 0.5f + 0.005f;
            float3 bottomEdgeVec = new float3(0, 1, 0) * (hull.height * 0.5f - hull.radius);
            float3 bottomPointVec = new float3(0, 1, 0) * (hull.height * 0.5f);


            float3 foot1 = node1.position + fudge + fudge + bottomPointVec;
            float3 foot2 = node2.position + fudge + fudge + bottomPointVec;

            if (hit1.hit == 1)
            {
                foot1 += new float3(0, 1, 0) * radiusMod + new float3(0, -1, 0) * hit1.distance;
            }

            if (hit2.hit == 1)
            {
                foot2 += new float3(0, 1, 0) * radiusMod + new float3(0, -1, 0) * hit2.distance;
            }

            _groundPos[offset] = foot1;
            _groundPos[offset + 1] = foot2;

            _caps[offset] = new CapsulecastCommand
            {
                point1 = foot1 + bottomEdgeVec,
                point2 = foot1 - bottomEdgeVec,
                direction = new float3(0, 0, 0),
                distance = 0f,
                layerMask = layerMask,
                radius = hull.radius
            };
            _caps[offset + 1] = new CapsulecastCommand
            {
                point1 = foot2 + bottomEdgeVec,
                point2 = foot2 - bottomEdgeVec,
                direction = new float3(0, 0, 0),
                distance = 0f,
                layerMask = layerMask,
                radius = hull.radius
            };
        }
    }
}

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
    public struct GroundCastsGen : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Node> _nodes;

        public NativeArray<Link> _links;

        [WriteOnly]
        public NativeArray<CapsulecastCommand> _caps;

        [ReadOnly]
        public HullData hull;

        [ReadOnly]
        public float3 fudge;


        public void Execute(Int32 index)
        {
            var link = _links[index];
            if (link.valid == 0) return;
            var node1 = _nodes[link.node1ID];
            if (node1.forbiddenHulls == hull.hullMask)
            {
                link.hullMask |= (HullMask)hull.hullMask;
                link.jumpHullMask |= (HullMask)hull.hullMask;
                _links[index] = link;
                return;
            }
            var node2 = _nodes[link.node2ID];
            if (node2.forbiddenHulls == hull.hullMask)
            {
                link.hullMask |= (HullMask)hull.hullMask;
                link.jumpHullMask |= (HullMask)hull.hullMask;
                _links[index] = link;
                return;
            }
            var offset = index * 2;

            float3 bottomEdgeVec = new float3(0, 1, 0) * (hull.height * 0.5f - hull.radius);
            float3 bottomPointVec = new float3(0, 1, 0) * (hull.height * 0.5f);
            Single radiusMod = hull.radius * 0.5f + 0.005f;


            float3 point = node1.position + fudge + new float3(0, 1, 0) * radiusMod;

            _caps[offset] = new CapsulecastCommand
            {
                point1 = (point + bottomEdgeVec),
                point2 = (point - bottomEdgeVec),
                radius = hull.radius,
                direction = new float3(0, -1, 0),
                distance = (radiusMod * 2f + 0.005f),
                layerMask = LayerIndex.world.mask
            };

            point = node2.position + fudge + new float3(0, 1, 0) * radiusMod;

            _caps[offset + 1] = new CapsulecastCommand
            {
                point1 = (point + bottomEdgeVec),
                point2 = (point - bottomEdgeVec),
                radius = hull.radius,
                direction = new float3(0, -1, 0),
                distance = (radiusMod * 2f + 0.005f),
                layerMask = LayerIndex.world.mask
            };
        }
    }
}

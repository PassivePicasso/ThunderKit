using RoR2;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

namespace RainOfStages.Bake
{
    [BurstCompile]
    public struct GroundCastsReadGen2 : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Node> _nodes;
        public NativeArray<Link> _links;
        [ReadOnly]
        public NativeArray<RaycastHit> _hitRes;


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

            var offset = index * 2;
            var hit1 = _hitRes[offset];
            if (_hitRes[offset].collider != null)
            {
                link.hullMask |= (HullMask)hull.hullMask;
                if ((HullMask)hull.hullMask == HullMask.Human)
                {
                    link.hullMask |= HullMask.Golem;
                    link.hullMask |= HullMask.BeetleQueen;
                }
                else if ((HullMask)hull.hullMask == HullMask.Golem)
                {
                    link.hullMask |= HullMask.BeetleQueen;
                }
                _links[index] = link;
                return;
            }
            if (_hitRes[offset + 1].collider != null)
            {
                link.hullMask |= (HullMask)hull.hullMask;
                if ((HullMask)hull.hullMask == HullMask.Human)
                {
                    link.hullMask |= HullMask.Golem;
                    link.hullMask |= HullMask.BeetleQueen;
                }
                else if ((HullMask)hull.hullMask == HullMask.Golem)
                {
                    link.hullMask |= HullMask.BeetleQueen;
                }
                _links[index] = link;
                return;
            }
        }
    }
}

using RoR2.Navigation;
using System;

namespace RainOfStages.Stage
{
    [Serializable]
    public struct Link
    {
        public int nodeIndexA;
        public int nodeIndexB;
        public float distanceScore;
        public float maxSlope;
        public float minJumpHeight;
        public int hullMask;
        public int jumpHullMask;
        public byte gateIndex;

        public static implicit operator Link(RoR2.Navigation.NodeGraph.Link li) => new Link
        {
            nodeIndexA = li.nodeIndexA.nodeIndex,
            nodeIndexB = li.nodeIndexB.nodeIndex,
            distanceScore = li.distanceScore,
            maxSlope = li.maxSlope,
            minJumpHeight = li.minJumpHeight,
            hullMask = li.hullMask,
            jumpHullMask = li.jumpHullMask,
            gateIndex = li.gateIndex,
        };
        public static implicit operator RoR2.Navigation.NodeGraph.Link(Link li) => new RoR2.Navigation.NodeGraph.Link
        {
            nodeIndexA = new RoR2.Navigation.NodeGraph.NodeIndex { nodeIndex = li.nodeIndexA },
            nodeIndexB = new RoR2.Navigation.NodeGraph.NodeIndex { nodeIndex = li.nodeIndexB },
            distanceScore = li.distanceScore,
            maxSlope = li.maxSlope,
            minJumpHeight = li.minJumpHeight,
            hullMask = li.hullMask,
            jumpHullMask = li.jumpHullMask,
            gateIndex = li.gateIndex,
        };
    }
}
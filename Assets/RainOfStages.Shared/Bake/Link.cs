using RoR2;

namespace RainOfStages.Bake
{
    public struct Link
    {
        public int valid;
        public int hasLOS;

        public int node1ID;
        public int node2ID;

        public float distanceScore;
        public float minJumpHeight;

        public HullMask hullMask;
        public HullMask jumpHullMask;

        public int gateIndex;
    }
}
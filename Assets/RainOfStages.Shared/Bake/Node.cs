using RoR2.Navigation;
using Unity.Mathematics;

namespace RainOfStages.Bake
{
    public struct Node
    {
        public float3 position;

        public NodeFlags flags;

        public int forbiddenHulls;

        public int gateIndex;
    }
}

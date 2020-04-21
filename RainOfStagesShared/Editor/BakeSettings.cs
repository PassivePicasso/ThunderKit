using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RainOfStages
{
    [CreateAssetMenu]
    public class BakeSettings : ScriptableObject
    {
        public float AgentHeight = 2;
        public float AgentRadius = 2;
        public float AgentSlope = 45;
        public float AgentClimb = 1;
        public float VoxelSize = 0.5f;
        public int TileSize = 20;

    }
}
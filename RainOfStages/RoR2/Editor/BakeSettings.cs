using System;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;
#endif

namespace RainOfStages
{
    public class BakeSettings : ScriptableObject
    {
        public Boolean DebugMode;
        public bool showMesh;


        public Material DebugMaterial;
        public int agentTypeID;
        public float agentRadius;
        public float agentHeight;
        public float agentSlope;
        public float agentClimb;
        public float minRegionArea;
        public bool overrideVoxelSize;
        public float voxelSize;
        public bool overrideTileSize;
        public int tileSize;
        public Vector3 globalNavigationOffset;

        [HideInInspector]
        public GameObject NodeMeshObject;

        public NavMeshBuildSettings bakeSettings => new NavMeshBuildSettings
        {
            agentClimb = agentClimb,
            agentHeight = agentHeight,
            agentRadius = agentRadius,
            agentSlope = agentSlope,
            agentTypeID = agentTypeID,
            minRegionArea = minRegionArea,
            overrideTileSize = overrideTileSize,
            overrideVoxelSize = overrideVoxelSize,
            tileSize = tileSize
        };

        private void OnValidate()
        {
            if (NodeMeshObject)
                NodeMeshObject.SetActive(showMesh);
        }

    }
}
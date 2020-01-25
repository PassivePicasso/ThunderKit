// Decompiled with JetBrains decompiler
// Type: RainOfStages.MapNode
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5CDEE1C8-AFDF-42E2-A8DF-7BD1AE8DC681
// Assembly location: F:\Projects\RoR2_Modding\Risk of Rain 2\Risk of Rain 2_Data\Managed\Assembly-CSharp.dll

using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RainOfStages.Stage
{
    [ExecuteInEditMode]
    public class MapNode : MonoBehaviour
    {
        private static List<MapNode> _instances = new List<MapNode>();
        private static ReadOnlyCollection<MapNode> instancesReadOnly = MapNode._instances.AsReadOnly();
        public static readonly float maxConnectionDistance = 15f;
        public List<MapNode.Link> links = new List<MapNode.Link>();
        [Tooltip("The name of the nodegraph gate associated with this node. If the named gate is closed this node will be treated as though it does not exist.")]
        public string gateName = "";
        public HullMask forbiddenHulls;
        [EnumMask(typeof(NodeFlags))]
        public NodeFlags flags;

        public static ReadOnlyCollection<MapNode> instances
        {
            get
            {
                return MapNode.instancesReadOnly;
            }
        }

        public void OnEnable()
        {
            MapNode._instances.Add(this);
        }

        public void OnDisable()
        {
            MapNode._instances.Remove(this);
        }

        private void AddLink(
          MapNode nodeB,
          float distanceScore,
          float minJumpHeight,
          HullClassification hullClassification)
        {
            int index = this.links.FindIndex((Predicate<MapNode.Link>)(item => (UnityEngine.Object)item.nodeB == (UnityEngine.Object)nodeB));
            if (index == -1)
            {
                this.links.Add(new MapNode.Link() { nodeB = nodeB });
                index = this.links.Count - 1;
            }
            MapNode.Link link = this.links[index];
            link.distanceScore = Mathf.Max(link.distanceScore, distanceScore);
            link.minJumpHeight = Mathf.Max(link.minJumpHeight, minJumpHeight);
            link.hullMask |= 1 << (int)(hullClassification & (HullClassification)31);
            if ((double)minJumpHeight > 0.0)
                link.jumpHullMask |= 1 << (int)(hullClassification & (HullClassification)31);
            if (string.IsNullOrEmpty(link.gateName))
                link.gateName = nodeB.gateName;
            this.links[index] = link;
        }

        private void BuildGroundLinks(ReadOnlyCollection<MapNode> nodes, MapNode.MoveProbe moveProbe)
        {
            Vector3 position1 = this.transform.position;
            for (int index1 = 0; index1 < nodes.Count; ++index1)
            {
                MapNode node = nodes[index1];
                if (!((UnityEngine.Object)node == (UnityEngine.Object)this))
                {
                    Vector3 position2 = node.transform.position;
                    double connectionDistance = (double)MapNode.maxConnectionDistance;
                    float num1 = (float)(connectionDistance * connectionDistance);
                    float sqrMagnitude = (position2 - position1).sqrMagnitude;
                    if ((double)sqrMagnitude < (double)num1)
                    {
                        float distanceScore = Mathf.Sqrt(sqrMagnitude);
                        for (int index2 = 0; index2 < 3; ++index2)
                        {
                            moveProbe.SetHull((HullClassification)index2);
                            if ((this.forbiddenHulls & (HullMask)(1 << index2)) == HullMask.None && (node.forbiddenHulls & (HullMask)(1 << index2)) == HullMask.None)
                            {
                                Vector3 vector3_1 = Vector3.up * (moveProbe.testCharacterController.height * 0.5f);
                                Vector3 vector3_2 = Vector3.up * 0.01f;
                                Vector3 vector3_3 = moveProbe.GetGroundPosition(position1) + vector3_2;
                                Vector3 vector3_4 = moveProbe.GetGroundPosition(position2) + vector3_2;
                                Vector3 vector3_5 = vector3_3 + vector3_1;
                                Vector3 vector3_6 = vector3_1;
                                Vector3 vector3_7 = vector3_4 + vector3_6;
                                if (moveProbe.CapsuleOverlapTest(vector3_5) && moveProbe.CapsuleOverlapTest(vector3_7))
                                {
                                    int num2 = moveProbe.GroundTest(vector3_5, vector3_7, 6f) ? 1 : 0;
                                    float minJumpHeight = num2 == 0 ? moveProbe.JumpTest(vector3_5, vector3_7, 7.5f) : 0.0f;
                                    if (num2 != 0 || (double)minJumpHeight > 0.0 && (double)minJumpHeight < 10.0)
                                        this.AddLink(node, distanceScore, minJumpHeight, (HullClassification)index2);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void BuildAirLinks(ReadOnlyCollection<MapNode> nodes, MapNode.MoveProbe moveProbe)
        {
            Vector3 position1 = this.transform.position;
            for (int index1 = 0; index1 < nodes.Count; ++index1)
            {
                MapNode node = nodes[index1];
                if (!(node == (this)))
                {
                    Vector3 position2 = node.transform.position;
                    double num1 = (double)MapNode.maxConnectionDistance * 2.0;
                    float num2 = (float)(num1 * num1);
                    float sqrMagnitude = (position2 - position1).sqrMagnitude;
                    if ((double)sqrMagnitude < (double)num2)
                    {
                        float distanceScore = Mathf.Sqrt(sqrMagnitude);
                        for (int index2 = 0; index2 < 3; ++index2)
                        {
                            if ((this.forbiddenHulls & (HullMask)(1 << index2)) == HullMask.None && (node.forbiddenHulls & (HullMask)(1 << index2)) == HullMask.None)
                            {
                                moveProbe.SetHull((HullClassification)index2);
                                Vector3 startPosition = position1;
                                Vector3 endPosition = position2;
                                bool startCheck = moveProbe.CapsuleOverlapTest(startPosition);
                                bool endCheck = moveProbe.CapsuleOverlapTest(endPosition);
                                bool pathCheck = moveProbe.FlyTest(startPosition, endPosition, 6f);
                                if (startCheck && endCheck && pathCheck)
                                    this.AddLink(node, distanceScore, 0.0f, (HullClassification)index2);
                            }
                        }
                    }
                }
            }
        }

        private void BuildRailLinks(ReadOnlyCollection<MapNode> nodes, MapNode.MoveProbe moveProbe)
        {
            Vector3 position1 = this.transform.position;
            for (int index1 = 0; index1 < nodes.Count; ++index1)
            {
                MapNode node = nodes[index1];
                if (!((UnityEngine.Object)node == (UnityEngine.Object)this))
                {
                    Vector3 position2 = node.transform.position;
                    double num1 = (double)MapNode.maxConnectionDistance * 2.0;
                    float num2 = (float)(num1 * num1);
                    float sqrMagnitude = (position2 - position1).sqrMagnitude;
                    if ((double)sqrMagnitude < (double)num2)
                    {
                        float distanceScore = Mathf.Sqrt(sqrMagnitude);
                        for (int index2 = 0; index2 < 3; ++index2)
                        {
                            HullDef hullDef = HullDef.Find((HullClassification)index2);
                            if ((this.forbiddenHulls & (HullMask)(1 << index2)) == HullMask.None && (node.forbiddenHulls & (HullMask)(1 << index2)) == HullMask.None)
                            {
                                moveProbe.SetHull((HullClassification)index2);
                                Vector3 vector3_1 = position1;
                                Vector3 vector3_2 = position2;
                                if ((double)Vector3.Angle(Vector3.up, vector3_2 - vector3_1) > 50.0)
                                {
                                    vector3_1.y += hullDef.height;
                                    vector3_2.y += hullDef.height;
                                    if (moveProbe.CapsuleOverlapTest(vector3_1) && moveProbe.CapsuleOverlapTest(vector3_2) && moveProbe.FlyTest(vector3_1, vector3_2, 6f))
                                        this.AddLink(node, distanceScore, 0.0f, (HullClassification)index2);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void BuildLinks(ReadOnlyCollection<MapNode> nodes, MapNodeGroup.GraphType graphType)
        {
            this.links.Clear();
            Vector3 position = this.transform.position;
            MapNode.MoveProbe moveProbe = new MapNode.MoveProbe();
            moveProbe.Init();
            switch (graphType)
            {
                case MapNodeGroup.GraphType.Ground:
                    this.BuildGroundLinks(nodes, moveProbe);
                    break;
                case MapNodeGroup.GraphType.Air:
                    this.BuildAirLinks(nodes, moveProbe);
                    break;
                case MapNodeGroup.GraphType.Rail:
                    this.BuildRailLinks(nodes, moveProbe);
                    break;
            }
            foreach (MapNodeLink component in this.GetComponents<MapNodeLink>())
            {
                if ((bool)((UnityEngine.Object)component.other))
                {
                    MapNode.Link link = new MapNode.Link() { nodeB = component.other, distanceScore = Vector3.Distance(position, component.other.transform.position), minJumpHeight = component.minJumpHeight, gateName = component.gateName, hullMask = -1 };
                    bool flag = false;
                    for (int index = 0; index < this.links.Count; ++index)
                    {
                        if ((UnityEngine.Object)this.links[index].nodeB == (UnityEngine.Object)component.other)
                        {
                            this.links[index] = link;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                        this.links.Add(link);
                }
            }
            moveProbe.Destroy();
        }

        public bool TestLineOfSight(MapNode other)
        {
            return !Physics.Linecast(this.transform.position + Vector3.up, other.transform.position + Vector3.up, (int)LayerIndex.world.mask);
        }

        public bool TestNoCeiling()
        {
            return !Physics.Raycast(new Ray(this.transform.position, Vector3.up), float.PositiveInfinity, (int)LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
        }

        public bool TestTeleporterOK()
        {
            float num1 = 15f;
            int num2 = 20;
            float num3 = 7f;
            float num4 = 3f;
            float num5 = 360f / (float)num2;
            for (int index = 0; index < num2; ++index)
            {
                RaycastHit hitInfo;
                if (!Physics.Raycast(new Ray(this.transform.position + Quaternion.AngleAxis(num5 * (float)index, Vector3.up) * (Vector3.forward * num1) + Vector3.up * num3, Vector3.down), out hitInfo, num4 + num3, (int)LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                    return false;
            }
            Debug.DrawRay(this.transform.position, this.transform.up * 20f, Color.green, 15f);
            return true;
        }

        public struct Link
        {
            public MapNode nodeB;
            public float distanceScore;
            public float minJumpHeight;
            public int hullMask;
            public int jumpHullMask;
            public string gateName;
        }

        private class MoveProbe
        {
            private float testTimeStep = 0.06666667f;
            public CharacterController testCharacterController;

            public void Init()
            {
                GameObject gameObject = new GameObject();
                gameObject.name = "NodeGraphProbe";
                Transform transform = gameObject.transform;
                this.testCharacterController = gameObject.AddComponent<CharacterController>();
                this.testCharacterController.stepOffset = 0.5f;
                this.testCharacterController.slopeLimit = 60f;
            }

            public void SetHull(HullClassification hullClassification)
            {
                HullDef hullDef = HullDef.Find(hullClassification);
                this.testCharacterController.radius = hullDef.radius;
                this.testCharacterController.height = hullDef.height;
            }

            public void Destroy()
            {
                UnityEngine.Object.DestroyImmediate((UnityEngine.Object)this.testCharacterController.gameObject);
            }

            private static float DistanceXZ(Vector3 a, Vector3 b)
            {
                a.y = 0.0f;
                b.y = 0.0f;
                return Vector3.Distance(a, b);
            }

            public Vector3 GetGroundPosition(Vector3 footPosition)
            {
                Vector3 vector3_1 = Vector3.up * (this.testCharacterController.height * 0.5f - this.testCharacterController.radius);
                Vector3 vector3_2 = Vector3.up * (this.testCharacterController.height * 0.5f);
                Vector3 vector3_3 = footPosition + vector3_2;
                float num = (float)((double)this.testCharacterController.radius * 0.5 + 0.00499999988824129);
                Vector3 start = footPosition + Vector3.up * num;
                Vector3 vector3_4 = Vector3.up * num;
                Vector3 vector3_5 = vector3_3 + vector3_4;
                RaycastHit hitInfo;
                if (Physics.CapsuleCast(vector3_5 + vector3_1, vector3_5 - vector3_1, this.testCharacterController.radius, Vector3.down, out hitInfo, (float)((double)num * 2.0 + 0.00499999988824129), (int)LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    Vector3 vector3_6 = hitInfo.distance * Vector3.down;
                    return start + vector3_6;
                }
                Debug.DrawLine(start, start + Vector3.up * 100f, Color.red, 30f);
                return footPosition;
            }

            public bool CapsuleOverlapTest(Vector3 centerOfCapsule)
            {
                Vector3 vector3_1 = Vector3.up * (this.testCharacterController.height * 0.5f - this.testCharacterController.radius);
                Vector3 vector3_2 = Vector3.up * (this.testCharacterController.height * 0.5f);
                Vector3 point0 = centerOfCapsule + vector3_1;
                Vector3 point1 = centerOfCapsule - vector3_1;
                double radius = (double)this.testCharacterController.radius;
                LayerIndex layerIndex = LayerIndex.world;
                int mask1 = (int)layerIndex.mask;
                layerIndex = LayerIndex.defaultLayer;
                int mask2 = (int)layerIndex.mask;
                int layerMask = mask1 | mask2;
                return (uint)Physics.OverlapCapsule(point0, point1, (float)radius, layerMask, QueryTriggerInteraction.Ignore).Length <= 0U;
            }

            public bool FlyTest(Vector3 startPos, Vector3 endPos, float flySpeed)
            {
                Vector3 vector3_1 = Vector3.up * (this.testCharacterController.height * 0.5f - this.testCharacterController.radius);
                Vector3 point1 = startPos + vector3_1;
                Vector3 point2 = startPos - vector3_1;
                double radius = (double)this.testCharacterController.radius;
                Vector3 vector3_2 = endPos - startPos;
                Vector3 normalized = vector3_2.normalized;
                vector3_2 = endPos - startPos;
                double magnitude = (double)vector3_2.magnitude;
                int mask = (int)LayerIndex.world.mask;
                return !Physics.CapsuleCast(point1, point2, (float)radius, normalized, (float)magnitude, mask);
            }

            private void MoveCapsule(Vector3 displacement)
            {
                int num = (int)this.testCharacterController.Move(displacement);
            }

            private void SetCapsulePosition(Vector3 position)
            {
                this.testCharacterController.transform.position = position;
                Physics.SyncTransforms();
            }

            public bool GroundTest(
              Vector3 startCenterOfCapsulePos,
              Vector3 endCenterOfCapsulePos,
              float hSpeed)
            {
                this.MoveCapsule(Vector3.zero);
                Vector3 zero = Vector3.zero;
                double num1 = (double)MapNode.MoveProbe.DistanceXZ(startCenterOfCapsulePos, endCenterOfCapsulePos);
                this.SetCapsulePosition(startCenterOfCapsulePos + Vector3.up);
                int num2 = Mathf.CeilToInt((float)(num1 * 1.5) / hSpeed / this.testTimeStep);
                Vector3 vector3_1 = this.testCharacterController.transform.position;
                for (int index = 0; index < num2; ++index)
                {
                    Vector3 vector3_2 = endCenterOfCapsulePos - this.testCharacterController.transform.position;
                    if ((double)vector3_2.sqrMagnitude <= 0.25)
                        return true;
                    Vector3 vector3_3 = vector3_2;
                    vector3_3.y = 0.0f;
                    vector3_3.Normalize();
                    zero.x = vector3_3.x * hSpeed;
                    zero.z = vector3_3.z * hSpeed;
                    zero += Physics.gravity * this.testTimeStep;
                    this.MoveCapsule(zero * this.testTimeStep);
                    Vector3 position = this.testCharacterController.transform.position;
                    if (position == vector3_1)
                        return false;
                    vector3_1 = position;
                }
                return false;
            }

            public float JumpTest(
              Vector3 startCenterOfCapsulePos,
              Vector3 endCenterOfCapsulePos,
              float hSpeed)
            {
                float initialYspeed = Trajectory.CalculateInitialYSpeed(Trajectory.CalculateGroundTravelTime(hSpeed, MapNode.MoveProbe.DistanceXZ(startCenterOfCapsulePos, endCenterOfCapsulePos)), endCenterOfCapsulePos.y - startCenterOfCapsulePos.y);
                int num1 = (int)this.testCharacterController.Move(Vector3.zero);
                Vector3 vector3_1 = endCenterOfCapsulePos - startCenterOfCapsulePos;
                vector3_1.y = 0.0f;
                vector3_1.Normalize();
                Vector3 vector3_2 = vector3_1 * hSpeed;
                vector3_2.y = initialYspeed;
                double num2 = (double)MapNode.MoveProbe.DistanceXZ(startCenterOfCapsulePos, endCenterOfCapsulePos);
                this.SetCapsulePosition(startCenterOfCapsulePos);
                int num3 = Mathf.CeilToInt((float)(num2 * 1.5) / hSpeed / this.testTimeStep);
                float b = float.NegativeInfinity;
                Vector3 vector3_3 = this.testCharacterController.transform.position;
                for (int index = 0; index < num3; ++index)
                {
                    Vector3 vector3_4 = endCenterOfCapsulePos - this.testCharacterController.transform.position;
                    if ((double)vector3_4.sqrMagnitude <= 4.0)
                        return b - startCenterOfCapsulePos.y;
                    b = Mathf.Max(this.testCharacterController.transform.position.y, b);
                    Vector3 vector3_5 = vector3_4;
                    vector3_5.y = 0.0f;
                    vector3_5.Normalize();
                    vector3_2.x = vector3_5.x * hSpeed;
                    vector3_2.z = vector3_5.z * hSpeed;
                    vector3_2 += Physics.gravity * this.testTimeStep;
                    int num4 = (int)this.testCharacterController.Move(vector3_2 * this.testTimeStep);
                    Vector3 position = this.testCharacterController.transform.position;
                    if (position == vector3_3)
                        return 0.0f;
                    vector3_3 = position;
                }
                return 0.0f;
            }
        }
    }
}

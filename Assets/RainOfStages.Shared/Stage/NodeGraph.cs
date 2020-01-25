using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RainOfStages.Stage
{
    [CreateAssetMenu]
    public class NodeGraph : ScriptableObject
    {
        public Node[] nodes = Array.Empty<Node>();
        public Link[] links = Array.Empty<Link>();
        public List<string> gateNames = new List<string>() { "" };
        public bool[] openGates = new bool[256];
        public const float overheadDotLimit = 0.7071068f;

        public void Clear()
        {
            this.nodes = (Node[])Array.Empty<Node>();
            this.links = (Link[])Array.Empty<Link>();
            this.gateNames = new List<string>() { "" };
        }

        public void SetNodes(ReadOnlyCollection<MapNode> mapNodes, ReadOnlyCollection<SerializableBitArray> lineOfSightMasks)
        {
            this.Clear();
            Dictionary<MapNode, int> dictionary = new Dictionary<MapNode, int>();
            List<Node> nodeList = new List<Node>();
            List<Link> linkList = new List<Link>();
            for (int nodeIndex = 0; nodeIndex < mapNodes.Count; ++nodeIndex)
            {
                MapNode mapNode = mapNodes[nodeIndex];
                dictionary[mapNode] = nodeIndex;
            }
            for (int index1 = 0; index1 < mapNodes.Count; ++index1)
            {
                MapNode mapNode = mapNodes[index1];
                int nodeIndex = dictionary[mapNode];
                int count = linkList.Count;
                for (int index2 = 0; index2 < mapNode.links.Count; ++index2)
                {
                    MapNode.Link link = mapNode.links[index2];
                    if (!dictionary.ContainsKey(link.nodeB))
                        Debug.LogErrorFormat((UnityEngine.Object)link.nodeB, "[{0}] Node {1} was not registered.", (object)index2, (object)link.nodeB);
                    linkList.Add(new Link()
                    {
                        nodeIndexA = nodeIndex,
                        nodeIndexB = dictionary[link.nodeB],
                        distanceScore = link.distanceScore,
                        minJumpHeight = link.minJumpHeight,
                        hullMask = link.hullMask,
                        jumpHullMask = link.jumpHullMask,
                        gateIndex = this.RegisterGateName(link.gateName)
                    });
                }
                HullMask forbiddenHulls = mapNode.forbiddenHulls;
                for (HullClassification hullClassification = HullClassification.Human; hullClassification < HullClassification.Count; ++hullClassification)
                {
                    bool flag = false;
                    int num = 1 << (int)(hullClassification & (HullClassification)31);
                    List<MapNode.Link> links = mapNode.links;
                    for (int index2 = 0; index2 < links.Count; ++index2)
                    {
                        if ((links[index2].hullMask & num) != 0)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                        forbiddenHulls |= (HullMask)num;
                }
                nodeList.Add(new Node()
                {
                    position = mapNode.transform.position,
                    linkIndex = count,
                    listSize = (uint)mapNode.links.Count,
                    forbiddenHulls = (int)forbiddenHulls,
                    flags = (int)mapNode.flags,
                    bytes = lineOfSightMasks[index1].GetBytes(),
                    gateIndex = this.RegisterGateName(mapNode.gateName)
                });
            }
            this.nodes = nodeList.ToArray();
            this.links = linkList.ToArray();
        }

        public Vector3 GetQuadraticCoordinates(
          float t,
          Vector3 startPos,
          Vector3 apexPos,
          Vector3 endPos)
        {
            return Mathf.Pow(1f - t, 2f) * startPos + (float)(2.0 * (double)t * (1.0 - (double)t)) * apexPos + Mathf.Pow(t, 2f) * endPos;
        }

        private byte RegisterGateName(string gateName)
        {
            if (string.IsNullOrEmpty(gateName))
                return 0;
            int num = this.gateNames.IndexOf(gateName);
            if (num == -1)
            {
                num = this.gateNames.Count;
                if (num >= 256)
                {
                    Debug.LogErrorFormat((UnityEngine.Object)this, "Nodegraph cannot have more than 255 gate names. Nodegraph={0} gateName={1}", (object)this, (object)gateName);
                    num = 0;
                }
                else
                    this.gateNames.Add(gateName);
            }
            return (byte)num;
        }

    }
}

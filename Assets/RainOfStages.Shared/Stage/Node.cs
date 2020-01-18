using UnityEngine;
using System.Reflection;
using RoR2.Navigation;
using System;

namespace RainOfStages.Stage
{
    [Serializable]
    public struct Node
    {
        static FieldInfo bytesField;
        static Node()
        {
            bytesField = typeof(SerializableBitArray).GetField("bytes", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public Vector3 position;
        public int linkIndex;
        public uint listSize;
        public int forbiddenHulls;
        public int flags;
        public byte gateIndex;
        public byte[] bytes;

        public static implicit operator Node(NodeGraph.Node node) => new Node
        {
            position = node.position,
            linkIndex = node.linkListIndex.index,
            listSize = node.linkListIndex.size,
            forbiddenHulls = (int)node.forbiddenHulls,
            bytes = node.lineOfSightMask.GetBytes(),
            gateIndex = node.gateIndex,
            flags = (int)node.flags,
        };

        public static implicit operator NodeGraph.Node(Node node)
        {
            var nnode = new NodeGraph.Node
            {
                position = node.position,
                linkListIndex = new NodeGraph.LinkListIndex { index = node.linkIndex, size = node.listSize },
                forbiddenHulls = (RoR2.HullMask)node.forbiddenHulls,
                lineOfSightMask = new SerializableBitArray(node.bytes.Length),
                gateIndex = node.gateIndex,
                flags = (NodeFlags)node.flags,
            };
            bytesField.SetValue(nnode.lineOfSightMask, node.bytes);
            return nnode;
        }
    }
}
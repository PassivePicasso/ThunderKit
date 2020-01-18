using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using RoR2.Navigation;

namespace RainOfStages.Stage
{
    public class NodeGraphProxy : ScriptableObject
    {
        private static FieldInfo nodesField;
        private static FieldInfo linksField;
        private static FieldInfo gateNamesField;
        private static FieldInfo openGatesField;

        [SerializeField]
        public string[] nodes;
        [SerializeField]
        public string[] links;
        [SerializeField]
        public List<string> gateNames;
        [SerializeField]
        public bool[] openGates;

        static NodeGraphProxy()
        {
            var ndt = typeof(NodeGraph);
            nodesField = ndt.GetField(nameof(nodes), BindingFlags.NonPublic | BindingFlags.Instance);
            linksField = ndt.GetField(nameof(links), BindingFlags.NonPublic | BindingFlags.Instance);
            gateNamesField = ndt.GetField(nameof(gateNames), BindingFlags.NonPublic | BindingFlags.Instance);
            openGatesField = ndt.GetField(nameof(openGates), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static implicit operator NodeGraphProxy(NodeGraph nodeGraph)
        {
            var nodeGraphProxy = ScriptableObject.CreateInstance<NodeGraphProxy>();

            var nodes = (nodesField.GetValue(nodeGraph) as NodeGraph.Node[]).Select(n => (Node)n).Select(n => JsonUtility.ToJson(n)).ToArray();
            var links = (linksField.GetValue(nodeGraph) as NodeGraph.Link[]).Select(n => (Link)n).Select(n => JsonUtility.ToJson(n)).ToArray();

            nodeGraphProxy.nodes = nodes;
            nodeGraphProxy.links = links;
            nodeGraphProxy.gateNames = gateNamesField.GetValue(nodeGraph) as List<string>;
            nodeGraphProxy.openGates = openGatesField.GetValue(nodeGraph) as bool[];

            return nodeGraphProxy;
        }

        public static implicit operator NodeGraph(NodeGraphProxy nodeGraphProxy)
        {
            var nodegraph = ScriptableObject.CreateInstance<RoR2.Navigation.NodeGraph>();

            NodeGraph.Node[] nodes = nodeGraphProxy.nodes.Select(n => (NodeGraph.Node)JsonUtility.FromJson<Node>(n)).ToArray();
            NodeGraph.Link[] links = nodeGraphProxy.links.Select(l => (NodeGraph.Link)JsonUtility.FromJson<Link>(l)).ToArray();

            nodesField.SetValue(nodegraph, nodes);
            linksField.SetValue(nodegraph, links);
            gateNamesField.SetValue(nodegraph, nodeGraphProxy.gateNames);
            openGatesField.SetValue(nodegraph, nodeGraphProxy.openGates);
            return nodegraph;
        }

    }
}
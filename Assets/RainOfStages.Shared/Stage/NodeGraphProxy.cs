using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using RainOfStages;

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
            var ndt = typeof(RoR2.Navigation.NodeGraph);
            nodesField = ndt.GetField(nameof(nodes), BindingFlags.NonPublic | BindingFlags.Instance);
            linksField = ndt.GetField(nameof(links), BindingFlags.NonPublic | BindingFlags.Instance);
            gateNamesField = ndt.GetField(nameof(gateNames), BindingFlags.NonPublic | BindingFlags.Instance);
            openGatesField = ndt.GetField(nameof(openGates), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static implicit operator NodeGraphProxy(RoR2.Navigation.NodeGraph nodeGraph)
        {
            var nodeGraphProxy = ScriptableObject.CreateInstance<NodeGraphProxy>();

            var nodes = (nodesField.GetValue(nodeGraph) as RoR2.Navigation.NodeGraph.Node[]).Select(n => (Node)n).Select(n => JsonUtility.ToJson(n)).ToArray();
            var links = (linksField.GetValue(nodeGraph) as RoR2.Navigation.NodeGraph.Link[]).Select(n => (Link)n).Select(n => JsonUtility.ToJson(n)).ToArray();

            nodeGraphProxy.nodes = nodes;
            nodeGraphProxy.links = links;
            nodeGraphProxy.gateNames = gateNamesField.GetValue(nodeGraph) as List<string>;
            nodeGraphProxy.openGates = openGatesField.GetValue(nodeGraph) as bool[];

            return nodeGraphProxy;
        }

        public static implicit operator NodeGraphProxy(NodeGraph nodeGraph)
        {
            var nodeGraphProxy = ScriptableObject.CreateInstance<NodeGraphProxy>();

            var nodes = nodeGraph.nodes.Select(n => JsonUtility.ToJson(n)).ToArray();
            var links = nodeGraph.links.Select(n => JsonUtility.ToJson(n)).ToArray();

            nodeGraphProxy.nodes = nodes;
            nodeGraphProxy.links = links;
            nodeGraphProxy.gateNames = nodeGraph.gateNames;
            nodeGraphProxy.openGates = nodeGraph.openGates;

            return nodeGraphProxy;
        }

        public static implicit operator NodeGraph(NodeGraphProxy nodeGraphProxy)
        {
            var nodeGraph = ScriptableObject.CreateInstance<NodeGraph>();

            var nodes = nodeGraphProxy.nodes.Select(n => JsonUtility.FromJson<Node>(n)).ToArray();
            var links = nodeGraphProxy.links.Select(n => JsonUtility.FromJson<Link>(n)).ToArray();

            nodeGraph.nodes = nodes;
            nodeGraph.links = links;
            nodeGraph.gateNames = nodeGraphProxy.gateNames;
            nodeGraph.openGates = nodeGraphProxy.openGates;

            return nodeGraph;
        }

        public static implicit operator RoR2.Navigation.NodeGraph(NodeGraphProxy nodeGraphProxy)
        {
            var nodegraph = ScriptableObject.CreateInstance<RoR2.Navigation.NodeGraph>();

            RoR2.Navigation.NodeGraph.Node[] nodes = nodeGraphProxy.nodes.Select(n => (RoR2.Navigation.NodeGraph.Node)JsonUtility.FromJson<Node>(n)).ToArray();
            RoR2.Navigation.NodeGraph.Link[] links = nodeGraphProxy.links?.Select(l => (RoR2.Navigation.NodeGraph.Link)JsonUtility.FromJson<Link>(l)).ToArray() ?? new RoR2.Navigation.NodeGraph.Link[0];

            nodesField.SetValue(nodegraph, nodes);
            linksField.SetValue(nodegraph, links);
            gateNamesField.SetValue(nodegraph, nodeGraphProxy.gateNames);
            openGatesField.SetValue(nodegraph, nodeGraphProxy.openGates);
            return nodegraph;
        }

    }
}
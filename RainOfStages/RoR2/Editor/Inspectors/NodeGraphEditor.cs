using RoR2.Navigation;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodeGraph))]
public class NodeGraphEditor : Editor
{
    static FieldInfo nodesField = typeof(NodeGraph).GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    private void OnSceneGUI()
    {
        var nodeGraph = target as NodeGraph;

        var nodes = nodesField.GetValue(nodeGraph) as RoR2.Navigation.NodeGraph.Node[];

        foreach (var node in nodes)
        {
            Handles.DrawSphere(0, node.position, Quaternion.identity, 1);
        }
    }
}
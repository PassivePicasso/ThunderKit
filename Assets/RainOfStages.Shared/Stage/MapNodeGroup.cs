// Decompiled with JetBrains decompiler
// Type: RainOfStages.MapNodeGroup
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5CDEE1C8-AFDF-42E2-A8DF-7BD1AE8DC681
// Assembly location: F:\Projects\RoR2_Modding\Risk of Rain 2\Risk of Rain 2_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RainOfStages.Stage
{
  public class MapNodeGroup : MonoBehaviour
  {
    public NodeGraph nodeGraph;
    public Transform testPointA;
    public Transform testPointB;
    public MapNodeGroup.GraphType graphType;

    public void Clear()
    {
      for (int index = this.transform.childCount - 1; index >= 0; --index)
        Object.DestroyImmediate((Object) this.transform.GetChild(index).gameObject);
    }

    public void AddNode(Vector3 position)
    {
      GameObject gameObject = new GameObject();
      gameObject.transform.position = position;
      gameObject.transform.parent = this.transform;
      gameObject.AddComponent<MapNode>();
      gameObject.name = "MapNode";
    }

    public List<MapNode> GetNodes()
    {
      List<MapNode> mapNodeList = new List<MapNode>();
      for (int index = this.transform.childCount - 1; index >= 0; --index)
      {
        MapNode component = this.transform.GetChild(index).GetComponent<MapNode>();
        if ((bool) ((Object) component))
          mapNodeList.Add(component);
      }
      return mapNodeList;
    }

    public void UpdateNoCeilingMasks()
    {
      int num = 0;
      foreach (MapNode node in this.GetNodes())
      {
        node.flags &= ~NodeFlags.NoCeiling;
        if (node.TestNoCeiling())
        {
          ++num;
          node.flags |= NodeFlags.NoCeiling;
        }
      }
      Debug.LogFormat("{0} successful ceiling masks baked.", (object) num);
    }

    public void UpdateTeleporterMasks()
    {
      int num = 0;
      foreach (MapNode node in this.GetNodes())
      {
        node.flags &= ~NodeFlags.TeleporterOK;
        if (node.TestTeleporterOK())
        {
          ++num;
          node.flags |= NodeFlags.TeleporterOK;
        }
      }
      Debug.LogFormat("{0} successful teleporter masks baked.", (object) num);
    }

    public void Bake(NodeGraph nodeGraph)
    {
      List<MapNode> nodes = this.GetNodes();
      ReadOnlyCollection<MapNode> readOnlyCollection = nodes.AsReadOnly();
      for (int index = 0; index < nodes.Count; ++index)
        nodes[index].BuildLinks(readOnlyCollection, this.graphType);
      List<SerializableBitArray> serializableBitArrayList = new List<SerializableBitArray>();
      for (int index1 = 0; index1 < nodes.Count; ++index1)
      {
        MapNode mapNode = nodes[index1];
        SerializableBitArray serializableBitArray = new SerializableBitArray(nodes.Count);
        for (int index2 = 0; index2 < nodes.Count; ++index2)
        {
          MapNode other = nodes[index2];
          serializableBitArray[index2] = mapNode.TestLineOfSight(other);
        }
        serializableBitArrayList.Add(serializableBitArray);
      }
      nodeGraph.SetNodes(readOnlyCollection, serializableBitArrayList.AsReadOnly());
    }

    public enum GraphType
    {
      Ground,
      Air,
      Rail,
    }
  }
}

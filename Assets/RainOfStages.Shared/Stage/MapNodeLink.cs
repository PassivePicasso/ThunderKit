// Decompiled with JetBrains decompiler
// Type: RainOfStages.MapNodeLink
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5CDEE1C8-AFDF-42E2-A8DF-7BD1AE8DC681
// Assembly location: F:\Projects\RoR2_Modding\Risk of Rain 2\Risk of Rain 2_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace RainOfStages.Stage
{
  [RequireComponent(typeof (MapNode))]
  public class MapNodeLink : MonoBehaviour
  {
    [Tooltip("The gate name associated with this link. If the named gate is closed, this link will not be used in pathfinding.")]
    public string gateName = "";
    public MapNode other;
    public float minJumpHeight;
    public GameObject[] objectsToEnableDuringTest;
    public GameObject[] objectsToDisableDuringTest;

    private void OnValidate()
    {
      if ((Object) this.other == (Object) this)
      {
        Debug.LogWarning((object) "Map node link cannot link a node to itself.");
        this.other = (MapNode) null;
      }
      if (!(bool) ((Object) this.other) || !((Object) this.other.GetComponentInParent<MapNodeGroup>() != (Object) this.GetComponentInParent<MapNodeGroup>()))
        return;
      Debug.LogWarning((object) "Map node link cannot link to a node in a separate node group.");
      this.other = (MapNode) null;
    }

    private void OnDrawGizmos()
    {
      if (!(bool) ((Object) this.other))
        return;
      Vector3 position1 = this.transform.position;
      Vector3 position2 = this.other.transform.position;
      Vector3 vector3 = (position1 + position2) * 0.5f;
      Color yellow = Color.yellow;
      yellow.a = 0.5f;
      Gizmos.color = Color.yellow;
      Gizmos.DrawLine(position1, vector3);
      Gizmos.color = yellow;
      Gizmos.DrawLine(vector3, position2);
    }
  }
}

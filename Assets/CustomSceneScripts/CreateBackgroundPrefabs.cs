using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainOfStages.CustomScene
{
    public class CreateBackgroundPrefabs : MonoBehaviour
    {
        public Transform WorldRoot;
        public Transform BackgroundContainer;

        public Transform[] Prefabs;

        public int distance = 100;
        public float buffer = 50;

        public float chance = 0.99f;

        // Start is called before the first frame update
        void Start()
        {
            var bounds = WorldRoot.GetComponentsInChildren<MeshRenderer>().Select(mr => mr.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
            bounds.Expand(buffer);

            var allEleemnts = Prefabs;
            int xWidth = (int)bounds.extents.x;
            int zWidth = (int)bounds.extents.z;
            int totalCreated = 0;
            for (int x = -xWidth - distance; x < xWidth + distance; x++)
            {
                for (int z = -zWidth - distance; z < zWidth + distance; z++)
                {
                    Vector3 targetPosition = new Vector3(x, 0, z);
                    if (bounds.Contains(targetPosition)) continue;
                    if (Random.value > chance) continue;

                    var nextPrefab = allEleemnts[Random.Range(0, allEleemnts.Length)];

                    var newInstance = Instantiate(nextPrefab);
                    newInstance.transform.parent = BackgroundContainer;
                    newInstance.transform.position = targetPosition + (newInstance.GetComponent<MeshRenderer>().bounds.extents.y * Vector3.down);
                    totalCreated++;
                }
            }

            Debug.Log($"Created {totalCreated} background prefabs");
        }
    }
}
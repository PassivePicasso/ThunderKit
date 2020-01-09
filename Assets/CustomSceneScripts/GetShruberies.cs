using System.Linq;
using UnityEngine;

namespace RainOfStages.CustomScene
{
    public class GetShruberies : MonoBehaviour
    {
        public Transform Surface;

        public Transform[] Trees;

        public Transform[] Blockers;

        public float chance = 0.99f;

        // Start is called before the first frame update
        void Start()
        {
            var bounds = Surface.GetComponentsInChildren<MeshRenderer>().Select(mr => mr.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });

            var allEleemnts = Trees.Union(Blockers).ToArray();
            int xWidth = (int)bounds.extents.x;
            int zWidth = (int)bounds.extents.z;
            int totalCreated = 0;
            for (int x = -xWidth; x < xWidth; x++)
            {
                for (int z = (int)-zWidth; z < zWidth; z++)
                {
                    if (Random.value > chance) continue;
                    if (Physics.Raycast(new Ray(new Vector3(x, 100, z), Vector3.down), out RaycastHit hitInfo, 150, LayerMask.GetMask("World")))
                    {

                        var nextPrefab = allEleemnts[Random.Range(0, allEleemnts.Length)];

                        var targetPosition = hitInfo.point;

                        var newInstance = Instantiate(nextPrefab);
                        newInstance.transform.parent = Surface;
                        newInstance.transform.position = targetPosition;
                        totalCreated++;
                    }
                }
            }
            Debug.Log($"THE KNIGHTS OF NEE WANT {totalCreated} Shrubberies! NEE!");
            Debug.Log("nee, nee(nee) nee! NEE! nee...");
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}
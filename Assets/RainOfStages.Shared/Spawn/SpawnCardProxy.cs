using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace RainOfStages.Proxies
{
    [CreateAssetMenu(menuName = "SpawnCardProxy")]
    public class SpawnCardProxy : ScriptableObject
    {
        public GameObject prefab;
        public bool sendOverNetwork;
        public HullClassification hullSize;
        public MapNodeGroup.GraphType nodeGraphType;

        [EnumMask(typeof(NodeFlags))]
        public NodeFlags requiredFlags;

        [EnumMask(typeof(NodeFlags))]
        public NodeFlags forbiddenFlags;
        public bool occupyPosition;

        public SpawnCard ToSpawnCard()
        {
            var spawnCard = ScriptableObject.CreateInstance<SpawnCard>();

            spawnCard.forbiddenFlags = forbiddenFlags;
            spawnCard.hullSize = hullSize;
            spawnCard.nodeGraphType = nodeGraphType;
            spawnCard.occupyPosition = occupyPosition;
            spawnCard.prefab = prefab;
            spawnCard.requiredFlags = requiredFlags;
            spawnCard.sendOverNetwork = sendOverNetwork;
            spawnCard.name = name;

            return spawnCard;
        }
    }
}

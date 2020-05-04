using UnityEngine;
#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;
#endif

namespace RainOfStages.Proxy
{
    [CreateAssetMenu(menuName = "Rain of Stages/Body SpawnCard")]
    public class BodySpawnCard : RoR2.BodySpawnCard, IProxyReference<RoR2.SpawnCard>
    {

        void Awake()
        {
            if (Application.isEditor) return;
            var card = (RoR2.BodySpawnCard)ResolveProxy();

            prefab = card.prefab;
            sendOverNetwork = card.sendOverNetwork;
            hullSize = card.hullSize;
            nodeGraphType = card.nodeGraphType;
            requiredFlags = card.requiredFlags;
            forbiddenFlags = card.forbiddenFlags;
            directorCreditCost = card.directorCreditCost;
            occupyPosition = card.occupyPosition;
        }


        public RoR2.SpawnCard ResolveProxy() => LoadCard<RoR2.BodySpawnCard>();

        private T LoadCard<T>() where T : RoR2.SpawnCard
        {
            var card = Resources.Load<T>($"SpawnCards/{typeof(T).Name}s/{name}");
            if (card == null)
                card = Resources.Load<T>($"spawncards/{typeof(T).Name.ToLower()}s/{name}");
            return card;
        }


#if UNITY_EDITOR
        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(BodySpawnCard))]
        public static void Create() => ScriptableHelper.CreateAsset<BodySpawnCard>();
#endif
    }
}
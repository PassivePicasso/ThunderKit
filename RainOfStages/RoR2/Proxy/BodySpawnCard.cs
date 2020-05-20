using UnityEngine;

namespace RainOfStages.Proxy
{
    public class BodySpawnCard : global::RoR2.BodySpawnCard, IProxyReference<global::RoR2.SpawnCard>
    {

        void Awake()
        {
            if (Application.isEditor) return;
            var card = (global::RoR2.BodySpawnCard)ResolveProxy();

            prefab = card.prefab;
            sendOverNetwork = card.sendOverNetwork;
            hullSize = card.hullSize;
            nodeGraphType = card.nodeGraphType;
            requiredFlags = card.requiredFlags;
            forbiddenFlags = card.forbiddenFlags;
            directorCreditCost = card.directorCreditCost;
            occupyPosition = card.occupyPosition;
        }


        public global::RoR2.SpawnCard ResolveProxy() => LoadCard<global::RoR2.BodySpawnCard>();

        private T LoadCard<T>() where T : global::RoR2.SpawnCard
        {
            var card = Resources.Load<T>($"SpawnCards/{typeof(T).Name}s/{name}");
            if (card == null)
                card = Resources.Load<T>($"spawncards/{typeof(T).Name.ToLower()}s/{name}");
            return card;
        }
    }
}
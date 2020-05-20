using UnityEngine;

namespace RainOfStages.Proxy
{
    public class InteractableSpawnCard : global::RoR2.InteractableSpawnCard, IProxyReference<global::RoR2.SpawnCard>
    {
        void Awake()
        {
            if (Application.isEditor) return;
            var card = (global::RoR2.InteractableSpawnCard)ResolveProxy();

            prefab = card.prefab;
            sendOverNetwork = card.sendOverNetwork;
            hullSize = card.hullSize;
            nodeGraphType = card.nodeGraphType;
            requiredFlags = card.requiredFlags;
            forbiddenFlags = card.forbiddenFlags;
            directorCreditCost = card.directorCreditCost;
            occupyPosition = card.occupyPosition;
            orientToFloor = card.orientToFloor;
            slightlyRandomizeOrientation = card.slightlyRandomizeOrientation;
            skipSpawnWhenSacrificeArtifactEnabled = card.skipSpawnWhenSacrificeArtifactEnabled;
        }

        public global::RoR2.SpawnCard ResolveProxy() => LoadCard<global::RoR2.InteractableSpawnCard>();

        private T LoadCard<T>() where T : global::RoR2.SpawnCard
        {
            var card = Resources.Load<T>($"spawncards/{typeof(T).Name.ToLower()}/{name}");
            if (card == null)
                card = Resources.Load<T>($"SpawnCards/{typeof(T).Name}/{name}");
            return card;
        }
    }
}
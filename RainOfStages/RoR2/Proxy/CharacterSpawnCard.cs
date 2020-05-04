using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;
#endif

namespace RainOfStages.Proxy
{
    public class CharacterSpawnCard : RoR2.CharacterSpawnCard, IProxyReference<RoR2.SpawnCard>
    {
        static FieldInfo runtimeLoadoutField = typeof(RoR2.CharacterSpawnCard).GetField("runtimeLoadout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        void Awake()
        {
            if (Application.isEditor) return;
            var card = (RoR2.CharacterSpawnCard)ResolveProxy();

            prefab = card.prefab;
            loadout = card.loadout;
            noElites = card.noElites;
            hullSize = card.hullSize;
            nodeGraphType = card.nodeGraphType;
            requiredFlags = card.requiredFlags;
            forbiddenFlags = card.forbiddenFlags;
            occupyPosition = card.occupyPosition;

            runtimeLoadoutField.SetValue(this, runtimeLoadoutField.GetValue(card));

            forbiddenAsBoss = card.forbiddenAsBoss;
            sendOverNetwork = card.sendOverNetwork;
            directorCreditCost = card.directorCreditCost;

        }
        public RoR2.SpawnCard ResolveProxy() => LoadCard<RoR2.CharacterSpawnCard>();

        private T LoadCard<T>() where T : RoR2.SpawnCard
        {
            var card = Resources.Load<T>($"spawncards/{typeof(T).Name.ToLower()}s/{name}");
            if (card == null)
                card = Resources.Load<T>($"SpawnCards/{typeof(T).Name}s/{name}");
            if (card == null)
                card = Resources.Load<T>($"spawncards/{typeof(T).Name.ToLower()}s/titan/{name}");
            if (card == null)
                card = Resources.Load<T>($"SpawnCards/{typeof(T).Name}s/Titan/{name}");
            return card;
        }
#if UNITY_EDITOR
        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(CharacterSpawnCard))]
        public static void Create() => ScriptableHelper.CreateAsset<CharacterSpawnCard>();
#endif
    }
}
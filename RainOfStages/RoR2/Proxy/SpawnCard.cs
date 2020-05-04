using UnityEngine;
#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;
#endif

namespace RainOfStages.Proxy
{
    [CreateAssetMenu(menuName = "Rain of Stages/SpawnCard")]
    public class SpawnCard : RoR2.SpawnCard
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(SpawnCard))]
        public static void Create() => ScriptableHelper.CreateAsset<SpawnCard>();
#endif
    }
}

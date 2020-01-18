using RoR2;
using UnityEngine;

namespace RainOfStages.Proxies
{
    //[CreateAssetMenu(menuName = "ROR2/DirectorCardProxy")]
    public class DirectorCardProxy : ScriptableObject
    {
        public bool allowAmbushSpawn = true;

        public int selectionWeight;
        public DirectorCore.MonsterSpawnDistance spawnDistance;
        public bool preventOverhead;
        public int minimumStageCompletions;
        public string requiredUnlockable;
        public string forbiddenUnlockable;

        public virtual DirectorCard ToDirectorCard() =>
            new DirectorCard
            {
                allowAmbushSpawn = allowAmbushSpawn,
                selectionWeight = selectionWeight,
                spawnDistance = spawnDistance,
                preventOverhead = preventOverhead,
                minimumStageCompletions = minimumStageCompletions,
                requiredUnlockable = requiredUnlockable,
                forbiddenUnlockable = forbiddenUnlockable
            };
    }

}
using RoR2;
using UnityEngine;

namespace RainOfStages.Proxies
{
    //[CreateAssetMenu(menuName = "ROR2/DirectorCardProxy")]
    public class DirectorCardProxy : ScriptableObject
    {
        public bool allowAmbushSpawn = true;

        //[Tooltip("Should not be zero! EVER.")]
        //public int cost;
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
                //cost = cost,
                selectionWeight = selectionWeight,
                spawnDistance = spawnDistance,
                preventOverhead = preventOverhead,
                minimumStageCompletions = minimumStageCompletions,
                requiredUnlockable = requiredUnlockable,
                forbiddenUnlockable = forbiddenUnlockable
            };
    }

}
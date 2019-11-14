using RoR2;
using UnityEngine;

namespace RainOfStages.Proxies
{
    [CreateAssetMenu(menuName = "ROR2/MonsterDirectorCard")]
    public class MonsterDirectorCard : DirectorCardProxy
    {
        public CharacterSpawnCardIndex spawnCard;

        public override DirectorCard ToDirectorCard()
        {
            var dc = base.ToDirectorCard();

            dc.spawnCard = Resources.Load<SpawnCard>($"SpawnCards/CharacterSpawnCards/{spawnCard}");

            return dc;
        }
        public enum CharacterSpawnCardIndex
        {
            cscArchWisp,
            cscBackupDrone,
            cscBeetle,
            cscBeetleGuard,
            cscBeetleGuardAlly,
            cscBeetleQueen,
            cscBell,
            cscBison,
            cscClayBoss,
            cscClayBruiser,
            cscElectricWorm,
            cscGolem,
            cscGravekeeper,
            cscGreaterWisp,
            cscHermitCrab,
            cscImp,
            cscImpBoss,
            cscJellyfish,
            cscLemurian,
            cscLemurianBruiser,
            cscLesserWisp,
            cscMagmaWorm,
            cscRoboBallBoss,
            cscRoboBallMini,
            cscSuperRoboBallBoss,
            cscTitanGold,
            cscTitanGoldAlly,
            cscVagrant,
            cscVulture
        }
    }
}

using RoR2;
using UnityEngine;

namespace RainOfStages.Proxies
{
    [CreateAssetMenu(menuName = "ROR2/InteractableDirectorCardProxy")]
    public class InteractableDirectorCard : DirectorCardProxy
    {
        public InteractableSpawnCardIndex spawnCard;

        public override DirectorCard ToDirectorCard()
        {
            var dc = base.ToDirectorCard();
            dc.spawnCard = Resources.Load<InteractableSpawnCard>($"SpawnCards/InteractableSpawnCard/{spawnCard}");

            return dc;
        }

        public enum InteractableSpawnCardIndex
        {
            iscBarrel1,
            iscBrokenDrone1,
            iscBrokenDrone2,
            iscBrokenEquipmentDrone,
            iscBrokenFlameDrone,
            iscBrokenMegaDrone,
            iscBrokenMissileDrone,
            iscBrokenTurret1,
            iscCategoryChestDamage,
            iscCategoryChestHealing,
            iscCategoryChestUtility,
            iscChest1,
            iscChest1Stealthed,
            iscChest2,
            iscDuplicator,
            iscDuplicatorLarge,
            iscDuplicatorMilitary,
            iscEquipmentBarrel,
            iscGoldChest,
            iscGoldshoresBeacon,
            iscGoldshoresPortal,
            iscLockbox,
            iscLunarChest,
            iscMSPortal,
            iscRadarTower,
            iscShopPortal,
            iscShrineBlood,
            iscShrineBoss,
            iscShrineChance,
            iscShrineCombat,
            iscShrineGoldshoresAccess,
            iscShrineHealing,
            iscShrineRestack,
            iscTeleporter,
            iscTripleShop,
            iscTripleShopLarge
        }
    }
}
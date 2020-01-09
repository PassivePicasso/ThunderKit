#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using static RainOfStages.Proxies.InteractableDirectorCard;
using RainOfStages.Proxies;
using System.IO;

namespace RainOfStages.Editor
{
    public class MenuCommands : ScriptableObject
    {
        private const string CharacterCardRootFolder = "Assets/CustomScene/Resources/DirectorCard_Character";

        [MenuItem("Tools/Rain of Stages/Generate Interactable Default Cards")]
        static void GenerateCards()
        {
            var rootFolder = "Assets/Resources/DirectorCard_Interactable";
            var defaultFolder = Path.Combine(rootFolder, "Default");

            AssetDatabase.DeleteAsset(defaultFolder);

            string defaultGuid = AssetDatabase.CreateFolder(rootFolder, "Default");


            var shopsFolderGuid = AssetDatabase.CreateFolder(defaultFolder, "Shops");
            var dronesFolderGuid = AssetDatabase.CreateFolder(defaultFolder, "Drones");
            var portalsFolderGuid = AssetDatabase.CreateFolder(defaultFolder, "Portals");
            var shrinesFolderGuid = AssetDatabase.CreateFolder(defaultFolder, "Shrines");
            //var otherFolderGuid = AssetDatabase.CreateFolder(defaultFolder, "Other");

            string shopsFolder = AssetDatabase.GUIDToAssetPath(shopsFolderGuid);
            string dronesFolder = AssetDatabase.GUIDToAssetPath(dronesFolderGuid);
            string portalsFolder = AssetDatabase.GUIDToAssetPath(portalsFolderGuid);
            string shrinesFolder = AssetDatabase.GUIDToAssetPath(shrinesFolderGuid);
            //string otherFolder = AssetDatabase.GUIDToAssetPath(otherFolderGuid);

            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscBarrel1, 10);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscChest1, 30);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscChest2, 4);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscEquipmentBarrel, 2);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscTripleShop, 8);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscTripleShopLarge, 1);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscGoldChest, 2);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscLunarChest, 1);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscChest1Stealthed, 6);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscDuplicator, 30);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscDuplicatorLarge, 6);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscDuplicatorMilitary, 1);
            BuildCard(shopsFolder, InteractableSpawnCardIndex.iscRadarTower, 2);

            BuildCard(shrinesFolder, InteractableSpawnCardIndex.iscShrineHealing, 7);
            BuildCard(shrinesFolder, InteractableSpawnCardIndex.iscShrineCombat, 10);
            BuildCard(shrinesFolder, InteractableSpawnCardIndex.iscShrineBlood, 10);
            BuildCard(shrinesFolder, InteractableSpawnCardIndex.iscShrineBoss, 3);
            BuildCard(shrinesFolder, InteractableSpawnCardIndex.iscShrineChance, 15);
            BuildCard(shrinesFolder, InteractableSpawnCardIndex.iscShrineGoldshoresAccess, 1);
            BuildCard(shrinesFolder, InteractableSpawnCardIndex.iscShrineRestack, 1);


            BuildCard(dronesFolder, InteractableSpawnCardIndex.iscBrokenDrone1, 5);
            BuildCard(dronesFolder, InteractableSpawnCardIndex.iscBrokenMegaDrone, 1);
            BuildCard(dronesFolder, InteractableSpawnCardIndex.iscBrokenMissileDrone, 7);
            BuildCard(dronesFolder, InteractableSpawnCardIndex.iscBrokenDrone2, 5);
            BuildCard(dronesFolder, InteractableSpawnCardIndex.iscBrokenTurret1, 5);
            BuildCard(dronesFolder, InteractableSpawnCardIndex.iscBrokenFlameDrone, 5);
            BuildCard(dronesFolder, InteractableSpawnCardIndex.iscBrokenEquipmentDrone, 5);

            BuildCard(portalsFolder, InteractableSpawnCardIndex.iscShopPortal, 5);
            BuildCard(portalsFolder, InteractableSpawnCardIndex.iscMSPortal, 5);
            BuildCard(portalsFolder, InteractableSpawnCardIndex.iscGoldshoresPortal, 5);
        }


        private static void BuildCard(string path, InteractableSpawnCardIndex interactableSpawnCard, int weight)
        {
            var card = CreateInstance<InteractableDirectorCard>();
            card.spawnCard = interactableSpawnCard;
            card.selectionWeight = weight;
            AssetDatabase.CreateAsset(card, $"{path}/{card.spawnCard}.asset");
        }
    }
}
#endif
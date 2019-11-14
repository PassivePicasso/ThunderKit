#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using static RainOfStages.Proxies.InteractableDirectorCard;
using RainOfStages.Proxies;

namespace RainOfStages.Editor
{
    public class MenuCommands : ScriptableObject
    {
        private const string InteractableCardRootFolder = "Assets/CustomScene/Resources/DirectorCard_Interactable";
        private const string CharacterCardRootFolder = "Assets/CustomScene/Resources/DirectorCard_Character";

        [MenuItem("Tools/Rain of Stages/Generate Interactable Default Cards")]
        static void GenerateCards()
        {
            EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");

            string guid = AssetDatabase.CreateFolder(InteractableCardRootFolder, "Default");
            string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);

            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscChest1, 15, 30);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscChest2, 30, 4);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscEquipmentBarrel, 1, 2);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscTripleShop, 20, 8);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscTripleShopLarge, 40, 1);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscGoldChest, 80, 2);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscLunarChest, 25, 1);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscBarrel1, 1, 10);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscShrineHealing, 15, 2);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscShrineCombat, 20, 3);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscShrineBlood, 20, 3);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscShrineBoss, 20, 1);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscShrineRestack, 30, 1);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscShrineChance, 20, 4);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscBrokenDrone1, 15, 5);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscBrokenMegaDrone, 40, 1);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscBrokenMissileDrone, 15, 7);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscBrokenDrone2, 15, 5);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscBrokenTurret1, 10, 5);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscChest1Stealthed, 10, 6);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscRadarTower, 1, 2);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscShrineGoldshoresAccess, 1, 2);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscDuplicator, 10, 30);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscDuplicatorLarge, 20, 6);
            BuildCard(newFolderPath, InteractableSpawnCardIndex.iscDuplicatorMilitary, 30, 1);

        }


        private static void BuildCard(string path, InteractableSpawnCardIndex interactableSpawnCard, int cost, int weight)
        {
            var card = CreateInstance<InteractableDirectorCard>();
            card.spawnCard = interactableSpawnCard;
            card.cost = cost;
            card.selectionWeight = weight;
            AssetDatabase.CreateAsset(card, $"{path}/{card.spawnCard}.asset");
        }
    }
}
#endif
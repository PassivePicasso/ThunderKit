using RoR2;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;
#endif

namespace RainOfStages.Campaign
{
    public class CampaignSegment : ScriptableObject
    {
        public List<SceneDef> Locations;
        public List<CampaignSegment> Destinations;

#if UNITY_EDITOR
        [MenuItem("Assets/Rain of Stages/Stages/" + nameof(CampaignSegment))]
        public static void Create() => ScriptableHelper.CreateAsset<CampaignSegment>();
#endif
    }
}
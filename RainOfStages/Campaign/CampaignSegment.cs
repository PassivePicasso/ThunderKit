using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace RainOfStages.Campaign
{
    [CreateAssetMenu(menuName = "Rain of Stages/Act")]
    public class CampaignSegment : ScriptableObject
    {
        public List<SceneDef> Locations;
        public List<CampaignSegment> Destinations;
    }
}
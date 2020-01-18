using System;
using System.Collections.Generic;
using RainOfStages.Proxies;
using UnityEngine;

namespace RainOfStages.Campaign
{
    [CreateAssetMenu(menuName = "ROR2/Act")]
    public class CampaignSegment : ScriptableObject
    {
        public List<SceneDefProxy> Locations;
        public List<CampaignSegment> Destinations;
    }
}
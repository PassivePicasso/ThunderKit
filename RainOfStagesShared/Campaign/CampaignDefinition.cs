using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainOfStages.Campaign
{
    [CreateAssetMenu(menuName = "Rain of Stages/Campaign")]
    public class CampaignDefinition : ScriptableObject
    {
        public string Name;
        public Texture2D previewTexture;
        public string Description;

        public CampaignSegment StartSegment;

        private CampaignSegment currentSegment;

        private void Awake()
        {
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private void Run_onRunDestroyGlobal(Run obj) => currentSegment = null;

        public virtual IEnumerable<SceneDef> OverrideDestinations(IEnumerable<SceneDef> destinations) => destinations;

        public virtual SceneDef PickNextScene(Xoroshiro128Plus nextStageRng, Run run)
        {
            do
            {
                if (currentSegment?.Destinations?.Any() ?? false)
                    currentSegment = nextStageRng.NextElementUniform(currentSegment.Destinations);
                else
                    currentSegment = StartSegment;
            }
            while (!currentSegment.Locations.Any());

            var locations = OverrideDestinations(currentSegment.Locations)
                               .Where(sceneDef => sceneDef != run.nextStageScene)
                               .ToArray();

            var nextStage = nextStageRng.NextElementUniform(locations);

            return nextStage;
        }
    }
}
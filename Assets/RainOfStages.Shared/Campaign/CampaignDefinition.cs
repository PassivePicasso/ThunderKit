using RainOfStages.Proxies;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using System.Linq;

namespace RainOfStages.Campaign
{
    [CreateAssetMenu(menuName = "ROR2/Campaign")]
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

        public virtual IEnumerable<SceneDefProxy> OverrideDestinations(IEnumerable<SceneDefProxy> destinations) => destinations;

        public virtual SceneDef PickNextScene(Xoroshiro128Plus nextStageRng, Run run)
        {
            currentSegment = currentSegment?.NextSegment ?? StartSegment;

            var destinations = OverrideDestinations(currentSegment.Destinations)
                               .Where(sceneDef => sceneDef != run.nextStageScene)
                               .ToArray();
            var nextStage =  nextStageRng.NextElementUniform(destinations).ToSceneDef();


            return nextStage;
        }
    }
}
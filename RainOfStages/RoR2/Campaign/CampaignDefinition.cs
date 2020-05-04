using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;
#endif

namespace RainOfStages.Campaign
{
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

#if UNITY_EDITOR
        [MenuItem("Assets/Rain of Stages/Stages/" + nameof(CampaignDefinition))]
        public static void Create() => ScriptableHelper.CreateAsset<CampaignDefinition>();
#endif
    }
}
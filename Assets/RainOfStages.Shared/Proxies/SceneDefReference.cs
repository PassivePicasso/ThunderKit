using UnityEngine;
using RoR2;

namespace RainOfStages.Proxies
{
    [CreateAssetMenu(menuName = "ROR2/SceneDefReference")]
    public class SceneDefReference : SceneDefProxy
    {
        public enum BuiltInScene
        {
            bazaar,
            blackbeach,
            crystalworld,
            dampcavesimple,
            foggyswamp,
            frozenwall,
            goldshores,
            golemplains,
            goolake,
            lobby,
            logbook,
            mysteryspace,
            shipgraveyard,
            title,
            wispgraveyard
        }
        public BuiltInScene SceneName;

        public override SceneDef ToSceneDef()
        {
            var def = Resources.Load<SceneDef>($"SceneDefs/{SceneName}");

            return def;
        }
    }
}
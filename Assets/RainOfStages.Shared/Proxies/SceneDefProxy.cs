using UnityEngine;
using RoR2;

namespace RainOfStages.Proxies
{
    public abstract class SceneDefProxy : ScriptableObject
    {
        public abstract SceneDef ToSceneDef();

    }
}
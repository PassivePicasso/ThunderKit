using RoR2;
using System.Collections.Generic;

namespace RainOfStages.Proxy
{
    public class SceneDefinition : SceneDef
    {
        public List<SceneDefReference> reverseSceneNameOverrides;
        public List<SceneDefReference> destionationInjections;
    }
}
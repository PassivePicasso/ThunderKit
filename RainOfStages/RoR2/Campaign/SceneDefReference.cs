using RoR2;
#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;
#endif

namespace RainOfStages.Proxy
{
    public class SceneDefReference : SceneDef
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Rain of Stages/Stages/" + nameof(SceneDefReference))]
        public static void Create() => ScriptableHelper.CreateAsset<SceneDefReference>();
#endif
    }
}
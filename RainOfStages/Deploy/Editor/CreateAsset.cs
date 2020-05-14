#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;

namespace RainOfStages.Deploy
{
    public class CreateAsset
    {
        [MenuItem("Assets/Rain of Stages/Modding Assets/" + nameof(Deployment))]
        public static void Create() => ScriptableHelper.CreateAsset<Deployment>();
    }
}
#endif
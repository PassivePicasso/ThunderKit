#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;

namespace RainOfStages.Thunderstore
{
    public class CreateAsset
    {
        [MenuItem("Assets/Rain of Stages/Modding Assets/Manifest")]
        public static void Create() => ScriptableHelper.CreateAsset<Manifest>();
    }
}
#endif
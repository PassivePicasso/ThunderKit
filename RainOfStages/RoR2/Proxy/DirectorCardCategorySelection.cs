#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;
#endif

namespace RainOfStages.Proxy
{
    public class DirectorCardCategorySelection : RoR2.DirectorCardCategorySelection
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(DirectorCardCategorySelection))]
        public static void Create() => ScriptableHelper.CreateAsset<DirectorCardCategorySelection>();
#endif
    }
}
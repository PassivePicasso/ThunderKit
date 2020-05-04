using UnityEngine;
#if UNITY_EDITOR
using RainOfStages.Utilities;
using UnityEditor;
#endif

namespace RainOfStages.Proxy
{
    [CreateAssetMenu(menuName = "Rain of Stages/SurfaceDef")]
    public class SurfaceDef : RoR2.SurfaceDef
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Rain of Stages/" + nameof(SurfaceDef))]
        public static void Create() => ScriptableHelper.CreateAsset<SurfaceDef>();
#endif
    }
}
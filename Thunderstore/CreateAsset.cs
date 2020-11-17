#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Utilities;
using UnityEditor;

namespace PassivePicasso.ThunderKit.Thunderstore.Editor
{
    public class CreateAsset
    {
        [MenuItem("Assets/ThunderKit/Modding Assets/Manifest")]
        public static void Create() => ScriptableHelper.CreateAsset<Manifest>();
    }
}
#endif
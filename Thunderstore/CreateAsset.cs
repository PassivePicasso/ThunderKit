#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Utilities;
using UnityEditor;
namespace PassivePicasso.ThunderKit.Thunderstore.Editor
{
    using static ScriptableHelper;
    public class CreateAsset
    {
        [MenuItem(ThunderKitContextRoot + nameof(Manifest))]
        public static void Create() => CreateAsset<Manifest>();
    }
}
#endif
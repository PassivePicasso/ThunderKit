#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Utilities;
using UnityEditor;
namespace PassivePicasso.ThunderKit.Deploy.Editor
{
    using static ScriptableHelper;
    public class CreateAsset
    {
        [MenuItem(ThunderKitContextRoot + nameof(Deployment))]
        public static void Create() => SelectNewAsset<Deployment>();
    }
}
#endif
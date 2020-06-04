#if UNITY_EDITOR
using PassivePicasso.ThunderKit.Utilities;
using UnityEditor;

namespace PassivePicasso.ThunderKit.Deploy.Editor
{
    public class CreateAsset
    {
        [MenuItem("Assets/ThunderKit/Modding Assets/" + nameof(Deployment))]
        public static void Create() => ScriptableHelper.CreateAsset<Deployment>();
    }
}
#endif
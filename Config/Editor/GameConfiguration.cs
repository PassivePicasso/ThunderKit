using UnityEngine;

namespace PassivePicasso.ThunderKit.AutoConfig.Editor
{
    public class GameConfiguration : ScriptableObject
    {
        public string[] RequiredAssemblies;
        public string GameExecutable;
    }
}
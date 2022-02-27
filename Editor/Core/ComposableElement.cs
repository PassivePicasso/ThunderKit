using UnityEngine;

namespace ThunderKit.Core
{
    public class ComposableElement : ScriptableObject
    {
        private void Awake()
        {
            name = GetType().Name;
        }
    }
}
using UnityEngine;

namespace RainOfStages.Proxies
{
    [CreateAssetMenu(menuName = "ROR2/CategoryProxy")]
    public class CategoryProxy : ScriptableObject
    {
        public DirectorCardProxy[] cards;
        public float selectionWeight;
    }
}
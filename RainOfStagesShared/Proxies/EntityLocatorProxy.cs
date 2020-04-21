using RainOfStages.Proxy;
using RoR2;
using UnityEngine;

namespace RainOfStages.Proxy
{
    [DisallowMultipleComponent]
    public class EntityLocatorProxy : MonoBehaviourProxy<EntityLocator>
    {
        [Tooltip("The root gameobject of the entity.")]
        public GameObject entity;
    }
}
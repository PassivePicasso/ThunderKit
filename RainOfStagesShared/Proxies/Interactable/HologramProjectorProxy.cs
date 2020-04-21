using RainOfStages.Proxy;
using RoR2.Hologram;
using UnityEngine;

namespace RainOfStages.Interactable
{
    public class HologramProjectorProxy : MonoBehaviourProxy<HologramProjector>
    {
        public float displayDistance;
        public bool disableHologramRotation;
        public Transform hologramPivot;
    }
}
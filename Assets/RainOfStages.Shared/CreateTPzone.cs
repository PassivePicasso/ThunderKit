using UnityEngine;

namespace RainOfStages
{
    public class CreateTPzone : MonoBehaviour
    {
        public RoR2.MapZone.TriggerType triggerType;
        public RoR2.MapZone.ZoneType zoneType;
        private void Awake()
        {
            RoR2.MapZone mapZone = gameObject.AddComponent<RoR2.MapZone>();
            mapZone.triggerType = triggerType;
            mapZone.zoneType = zoneType;
            DestroyImmediate(gameObject.GetComponent<CreateTPzone>());
        }
    }
}
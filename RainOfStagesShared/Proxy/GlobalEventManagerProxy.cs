using UnityEngine;
using UnityEngine.Networking;

namespace RainOfStages
{
    public class GlobalEventManagerProxy : MonoBehaviour
    {
        private void Awake()
        {
            if (NetworkServer.active)
            {
                var gem = gameObject.AddComponent<RoR2.GlobalEventManager>();
                gem.AACannonMuzzleEffect = (GameObject)Resources.Load("prefabs/effects/muzzleflashes/muzzleflashaacannon");
                gem.AACannonPrefab = (GameObject)Resources.Load("prefabs/projectiles/aacannon");
                gem.chainLightingPrefab = (GameObject)Resources.Load("prefabs/projectiles/chainlightning");
                gem.daggerPrefab = (GameObject)Resources.Load("prefabs/projectiles/daggerprojectile");
                gem.explodeOnDeathPrefab = (GameObject)Resources.Load("prefabs/networkedobjects/willowispdelay");
                gem.healthOrbPrefab = (GameObject)Resources.Load("prefabs/networkedobjects/healthglobe");
                gem.missilePrefab = (GameObject)Resources.Load("prefabs/projectiles/missileprojectile");
                gem.plasmaCorePrefab = (GameObject)Resources.Load("prefabs/projectiles/plasmacore");
            }
            //Destroy(this.gameObject.GetComponent<CreateGM>());
            Debug.Log($"Completed {GetType().Name}");
        }
    }
}
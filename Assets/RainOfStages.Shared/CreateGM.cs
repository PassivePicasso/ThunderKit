using UnityEngine;
using UnityEngine.Networking;

namespace RainOfStages
{
    public class CreateGM : MonoBehaviour
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


                //??????????????????
                gem.healthOrbPrefab = (GameObject)Resources.Load("prefabs/networkedobjects/healthglobe");

                gem.missilePrefab = (GameObject)Resources.Load("prefabs/projectiles/missileprojectile");
                gem.plasmaCorePrefab = (GameObject)Resources.Load("prefabs/projectiles/plasmacore");
            }

            //var allResources = Resources.LoadAll("", typeof(GameObject));
            //var potentials = allResources.Cast<GameObject>().ToList();//.Where(go => go.name.Contains("Healthglobe"))
            //foreach (var potential in potentials)
            //    Debug.Log($"Found Health thing: {potential.name}");

            Destroy(this.gameObject.GetComponent<CreateGM>());
        }
    }
}
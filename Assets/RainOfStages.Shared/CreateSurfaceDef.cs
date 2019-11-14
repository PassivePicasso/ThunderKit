using UnityEngine;
using UnityEngine.Networking;

namespace RainOfStages
{
    public class CreateSurfaceDef : MonoBehaviour
    {
        private void Awake()
        {
            if (NetworkServer.active)
            {
                SurfaceDefProvider Provider = gameObject.AddComponent<SurfaceDefProvider>();
                Provider.surfaceDef = ScriptableObject.CreateInstance<SurfaceDef>();
                //Provider.surfaceDef.approximateColor = Color.green;

            }
            Destroy(gameObject.GetComponent<CreateSurfaceDef>());
        }
    }
}
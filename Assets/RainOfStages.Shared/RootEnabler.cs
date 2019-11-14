using UnityEngine;

namespace RainOfStages
{
    public class RootEnabler : MonoBehaviour
    {
        public GameObject[] gameObjects;

        public void StartScene()
        {
            foreach (var obj in gameObjects)
            {
                obj.SetActive(true);
            }

            Destroy(gameObject);
        }
    }
}
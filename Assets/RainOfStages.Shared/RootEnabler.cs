using UnityEngine;

namespace RainOfStages
{
    public class RootEnabler : MonoBehaviour
    {
        public GameObject[] gameObjects;

        private void Awake()
        {
            StartScene();
        }

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
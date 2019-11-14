using RoR2;
using UnityEngine;

namespace RainOfStages
{
    public class CameraOverride : MonoBehaviour
    {
        private GameObject cameraController, sceneCameraGameObject;
        private FlyCamera flyCamera;
        private CameraRigController rig;
        private Vector3 originalCameraPosition;
        private bool cursorOpened = false;
        // Update is called once per frame
        void Update()
        {
            if (!rig) rig = FindObjectOfType<CameraRigController>();
            if (!flyCamera)
            {
                sceneCameraGameObject = rig.sceneCam.gameObject;
                flyCamera = sceneCameraGameObject.AddComponent<FlyCamera>();
                flyCamera.enabled = false;
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                rig.enabled = !rig.enabled;
                UpdateCursor(cursorOpened);
                if (rig.enabled)
                    sceneCameraGameObject.transform.position = originalCameraPosition;
            }

            if (!rig.enabled)
            {
                if (Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    flyCamera.enabled = true;
                    UpdateCursor(cursorOpened);
                }
                else if (Input.GetKeyUp(KeyCode.LeftAlt))
                {
                    flyCamera.enabled = false;
                    UpdateCursor(cursorOpened);
                }

            }
        }
        void UpdateCursor(bool state)
        {
            if (state)
            {
                MPEventSystemManager.primaryEventSystem.cursorOpenerCount--;
                cursorOpened = false;
                Debug.Log("CursorOff");
            }
            else
            {
                MPEventSystemManager.primaryEventSystem.cursorOpenerCount++;
                cursorOpened = true;
                Debug.Log("CursorOn");
            }
        }
    }
}
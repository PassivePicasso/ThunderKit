using UnityEngine;

namespace RainOfStages
{
    public class FlyCamera : MonoBehaviour
    {
        private Vector3 _angles;
        public float speed = 0.1f;
        public float fastSpeed = 2.0f;
        public float mouseSpeed = 4.0f;

        private void OnEnable()
        {
            //Debug.Log(speed);
            speed = 0.1f;
            _angles = transform.eulerAngles;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable() { Cursor.lockState = CursorLockMode.None; }

        private void Update()
        {
            _angles.x -= Input.GetAxis("Mouse Y") * mouseSpeed;
            _angles.y += Input.GetAxis("Mouse X") * mouseSpeed;
            transform.eulerAngles = _angles;
            float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : speed;
            transform.position +=
                Input.GetAxis("Horizontal") * moveSpeed * transform.right +
                Input.GetAxis("Vertical") * moveSpeed * transform.forward;


            // addition: Scroll wheel speed control
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
            { speed *= 2; }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
            {
                speed *= 0.5f;
                //Debug.Log(speed);
            }
             
            float upDownMultiplier = 0.5f;
            if (Input.GetKey(KeyCode.E))
            { transform.position += moveSpeed * transform.up * upDownMultiplier; }

            if (Input.GetKey(KeyCode.Q))
            { transform.position -= moveSpeed * transform.up * upDownMultiplier; }

        }
    }
}
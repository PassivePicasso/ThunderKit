using RoR2;
using UnityEngine;

namespace RainOfStages.Behaviours
{
    [RequireComponent(typeof(BoxCollider))]
    public class JumpPad : MonoBehaviour
    {
        public int iterations = 20;
        [HideInInspector]
        public float jumpAngle = 45;
        [SerializeField]
        public Vector3 jumpVelocity;

        [SerializeField, HideInInspector]
        public Vector3 target;
        private Vector3 origin => transform.position;

        public string jumpSoundString;

        private Vector3 lastTargetPosition;
        private float lastJumpAngle = 45;

        public void OnTriggerEnter(Collider other)
        {
            RoR2.CharacterMotor motor = other.GetComponent<CharacterMotor>();
            if (!(bool)((UnityEngine.Object)motor) || !motor.hasEffectiveAuthority)
                return;
            if (!motor.disableAirControlUntilCollision)
            {
                int num = (int)Util.PlaySound(this.jumpSoundString, this.gameObject);
            }
            SetTargetWithAngle();
            motor.disableAirControlUntilCollision = true;
            motor.velocity += jumpVelocity;
            motor.Motor.ForceUnground();
        }

        void OnDrawGizmos()
        {
            var (_, offset, distance) = LoadVariables();
            DrawArc(iterations,
                    LaunchSpeed(distance, offset, Physics.gravity.magnitude, jumpAngle * Mathf.Deg2Rad),
                    distance, Physics.gravity.magnitude, jumpAngle * Mathf.Deg2Rad);

            var planarTargetPosition = new Vector3(target.x, origin.y, target.z);
            transform.forward = planarTargetPosition - origin;

        }

        public void SetTargetWithAngle()
        {
            var (_, offset, distance) = LoadVariables();

            if (jumpAngle == lastJumpAngle && lastTargetPosition == target) return;
            lastTargetPosition = target;
            lastJumpAngle = jumpAngle;

            var currentSpeed = LaunchSpeed(distance, offset, Physics.gravity.magnitude, jumpAngle * Mathf.Deg2Rad);

            DrawArc(iterations, currentSpeed, distance, Physics.gravity.magnitude, jumpAngle * Mathf.Deg2Rad);

            var velocityX = currentSpeed * Mathf.Cos(jumpAngle);
            var velocityY = currentSpeed * Mathf.Sin(jumpAngle);

            var planarTargetPosition = new Vector3(target.x, origin.y, target.z);
            transform.forward = planarTargetPosition - origin;
            jumpVelocity = transform.TransformVector(new Vector3(0, velocityY, velocityX));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + jumpVelocity);
        }

        public static float LaunchSpeed(float distance, float yOffset, float gravity, float angle) =>
            (distance * Mathf.Sqrt(gravity) * Mathf.Sqrt(1 / Mathf.Cos(angle))) / Mathf.Sqrt(2 * distance * Mathf.Sin(angle) + 2 * yOffset * Mathf.Cos(angle));

        //Projects a vector onto a plane. The output is not normalized.
        public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector) => vector - (Vector3.Dot(vector, planeNormal) * planeNormal);

        public void DrawArc(int iterations, float speed, float distance, float gravity, float angle)
        {
            if (iterations < 0) return;

            float iterationSize = distance / iterations;

            float radians = angle;

            Vector3[] points = new Vector3[iterations + 1];

            for (int i = 0; i <= iterations; i++)
            {
                float x = iterationSize * i;
                float t = x / (speed * Mathf.Cos(radians));
                float y = -0.5f * gravity * (t * t) + speed * Mathf.Sin(radians) * t;

                Vector3 p = new Vector3(0, y, x);

                points[i] = transform.TransformPoint(p);
            }

            for (int i = 0; i < points.Length - 1; i++) 
                Gizmos.DrawLine(points[i], points[i + 1]);
        }


        (Vector3 direction, float offset, float distance) LoadVariables()
        {
            Vector3 direction = target - origin;
            float yOffset = -direction.y;
            direction = ProjectVectorOnPlane(Vector3.up, direction);
            float distance = direction.magnitude;

            return (direction, yOffset, distance);
        }
    }
}
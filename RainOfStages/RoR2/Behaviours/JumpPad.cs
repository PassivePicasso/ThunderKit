using EntityStates.Missions.Arena.NullWard;
using RoR2;
using UnityEngine;

namespace RainOfStages.Behaviours
{
    [RequireComponent(typeof(Collider))]
    public class JumpPad : MonoBehaviour
    {
        public int iterations = 20;
        [HideInInspector]
        public float jumpAngle = 45;

        [SerializeField]
        public Vector3 jumpVelocity;

        public float time;

        [SerializeField, HideInInspector]
        public Vector3 destination;
        private Vector3 origin => transform.position;

        public string jumpSoundString;

        private Vector3 lastTargetPosition;
        private float lastJumpAngle = 45;

        public void OnTriggerStay(Collider other)
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
            motor.velocity = jumpVelocity;
            motor.Motor.ForceUnground();
        }

        void OnDrawGizmos()
        {
            SetTargetWithAngle();
            var (offset, planarOffset, normalPlanarOffset) = LoadVariables();
            float speed = Trajectory.CalculateGroundSpeed(time, planarOffset.magnitude);

            DrawArc(iterations, speed, offset.magnitude, Physics.gravity.magnitude, jumpAngle * Mathf.Deg2Rad);

            var planarTargetPosition = new Vector3(destination.x, origin.y, destination.z);
            transform.forward = planarTargetPosition - origin;

            Gizmos.DrawSphere(destination, 3);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + jumpVelocity);
        }

        public void SetTargetWithAngle()
        {
            if (jumpAngle == lastJumpAngle && lastTargetPosition == destination) return;
            var (offset, planarOffset, normalPlanarOffset) = LoadVariables();

            float planarVelocity = Trajectory.CalculateGroundSpeed(time, planarOffset.magnitude);
            float verticalVelocity = Trajectory.CalculateInitialYSpeed(time, offset.y);

            jumpVelocity = new Vector3(normalPlanarOffset.x * planarVelocity, verticalVelocity, normalPlanarOffset.z * planarVelocity);
        }

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


        (Vector3 offset, Vector3 planarOffset, Vector3 normalPlanarOffset) LoadVariables()
        {
            var offset = destination - origin;
            var planarOffset = Vector3.ProjectOnPlane(offset, Vector3.up);
            var normalPlanarOffset = planarOffset.normalized;

            return (offset, planarOffset, normalPlanarOffset);
        }
    }
}
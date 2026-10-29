using UnityEngine;
using VRPhysicsLab.Core;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Experimental friction cart/slider that moves along the track.
    /// Dynamic friction force (F_f = \u03bc * m * g) is applied opposite to velocity,
    /// producing textbook-accurate deceleration a = -\u03bc * g until coming to a complete stop.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FrictionCart : MonoBehaviour, IResetable
    {
        [Header("Configuration")]
        [Tooltip("Mass of the friction cart in kg.")]
        [SerializeField] private float mass = 1.0f;

        [Tooltip("Connected surface slot to read the active friction coefficient.")]
        [SerializeField] private SurfacePlateSlot surfaceSlot;

        [Tooltip("Track motion constraints (locks Y, Z and rotations).")]
        [SerializeField] private bool constrainToTrack = true;

        [Tooltip("Gravity acceleration constant in m/s\u00b2.")]
        [SerializeField] private float gravity = 9.81f;

        private Rigidbody rb;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool isLaunched = false;

        public float Mass => mass;
        public Rigidbody Rigidbody => rb != null ? rb : (rb = GetComponent<Rigidbody>());
        public bool IsLaunched => isLaunched;
        public float CurrentSpeed => rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            if (rb != null)
            {
                rb.mass = mass;
            }
        }

        private void Start()
        {
            ApplyTrackConstraints();
            if (surfaceSlot == null)
            {
                surfaceSlot = Object.FindFirstObjectByType<SurfacePlateSlot>();
            }
        }

        private void FixedUpdate()
        {
            if (rb == null) return;

            float vx = rb.linearVelocity.x;

            // Only apply friction while moving along the track
            if (Mathf.Abs(vx) > 0.005f)
            {
                float mu = 0.05f; // default low friction if no plate installed
                if (surfaceSlot != null && surfaceSlot.InstalledPlate != null)
                {
                    mu = surfaceSlot.InstalledPlate.FrictionCoefficient;
                }

                // F_friction = \u03bc * m * g
                // Deceleration = -\u03bc * g * sign(vx)
                float frictionDecel = mu * gravity;
                float deltaV = frictionDecel * Time.fixedDeltaTime;

                if (Mathf.Abs(vx) <= deltaV)
                {
                    // Velocity reached zero: come to a clean stop
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                else
                {
                    float newVx = vx - Mathf.Sign(vx) * deltaV;
                    rb.linearVelocity = new Vector3(newVx, 0f, 0f);
                }
            }
            else if (isLaunched && Mathf.Abs(vx) <= 0.005f)
            {
                // Fully stopped
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        public void ApplyTrackConstraints()
        {
            if (rb != null && constrainToTrack)
            {
                rb.constraints = RigidbodyConstraints.FreezePositionY |
                                 RigidbodyConstraints.FreezePositionZ |
                                 RigidbodyConstraints.FreezeRotation;
            }
        }

        /// <summary>
        /// Launches the cart with a specified initial impulse velocity.
        /// </summary>
        public void Launch(float initialVelocity)
        {
            if (rb == null) return;

            isLaunched = true;
            rb.linearVelocity = new Vector3(initialVelocity, 0f, 0f);
            rb.angularVelocity = Vector3.zero;
        }

        public void ResetToInitialState()
        {
            isLaunched = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = initialPosition;
                rb.rotation = initialRotation;
            }
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            ApplyTrackConstraints();
        }
    }
}

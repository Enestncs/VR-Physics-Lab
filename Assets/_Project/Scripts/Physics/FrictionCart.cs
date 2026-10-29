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
        private float currentSpeed = 0f;

        public float Mass => mass;
        public Rigidbody Rigidbody => rb != null ? rb : (rb = GetComponent<Rigidbody>());
        public bool IsLaunched => isLaunched;
        public float CurrentSpeed => isLaunched ? currentSpeed : (rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f);

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

            if (!isLaunched)
            {
                currentSpeed = 0f;
                return;
            }

            if (currentSpeed > 0.0001f)
            {
                float mu = 0.05f; // default low friction if no plate installed
                if (surfaceSlot != null && surfaceSlot.InstalledPlate != null)
                {
                    mu = surfaceSlot.InstalledPlate.FrictionCoefficient;
                }

                // F_friction = \u03bc * m * g  =>  Deceleration a = \u03bc * g
                float frictionDecel = mu * gravity;
                float dt = Time.fixedDeltaTime;
                float prevSpeed = currentSpeed;
                float nextSpeed = Mathf.Max(0f, prevSpeed - frictionDecel * dt);
                currentSpeed = nextSpeed;

                if (nextSpeed > 0f)
                {
                    // Set velocity to midpoint average over this step so PhysX integrates exact distance (v_k^2 - v_{k+1}^2) / (2a)
                    float avgVx = 0.5f * (prevSpeed + nextSpeed);
                    rb.linearVelocity = new Vector3(avgVx, 0f, 0f);
                    rb.angularVelocity = Vector3.zero;
                }
                else
                {
                    // Final partial step to come to rest at exact stopping distance
                    float remDist = (prevSpeed * prevSpeed) / (2f * frictionDecel);
                    Vector3 finalPos = rb.position + new Vector3(remDist, 0f, 0f);
                    rb.position = finalPos;
                    transform.position = finalPos;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                currentSpeed = 0f;
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
        /// Cannot be launched again until ResetToInitialState() is called.
        /// </summary>
        public void Launch(float initialVelocity)
        {
            if (rb == null) return;
            if (isLaunched) return;

            isLaunched = true;
            currentSpeed = Mathf.Abs(initialVelocity);
            rb.linearVelocity = new Vector3(currentSpeed, 0f, 0f);
            rb.angularVelocity = Vector3.zero;
        }

        public void ResetToInitialState()
        {
            isLaunched = false;
            currentSpeed = 0f;
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
            UnityEngine.Physics.SyncTransforms();
        }
    }
}

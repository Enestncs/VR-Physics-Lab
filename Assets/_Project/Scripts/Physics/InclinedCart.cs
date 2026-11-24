using UnityEngine;
using UnityEngine.Events;
using VRPhysicsLab.Core;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Physics cart riding along an AdjustableRamp.
    /// Released from rest (v0 = 0) at the top of the incline and accelerated by gravity component
    /// a = g * (sin(theta) - mu * cos(theta)) along the ramp's local +X axis.
    /// </summary>
    public class InclinedCart : MonoBehaviour, IResetable
    {
        [Header("Configuration")]
        [SerializeField] private AdjustableRamp ramp;
        [SerializeField] private float mass = 1.0f;
        [SerializeField] private float gravity = 9.81f;
        [Tooltip("Rolling friction coefficient along the ramp (0 = ideal frictionless plane).")]
        [SerializeField] private float rollingFriction = 0.0f;

        [Header("Track Positions (Local X relative to Ramp_Pivot)")]
        [Tooltip("Starting local X at the top release gate.")]
        [SerializeField] private float startLocalX = -1.32f;
        [Tooltip("Finish sensor local X near the bottom hinge (travel distance = endLocalX - startLocalX = 1.20m).")]
        [SerializeField] private float endLocalX = -0.12f;
        [Tooltip("Local Y offset of cart root above the ramp deck center.")]
        [SerializeField] private float deckLocalY = 0.052f;

        [Header("Events")]
        public UnityEvent onReleased;
        public UnityEvent onReachedBottom;

        private bool isReleased = false;
        private bool hasFinished = false;
        private float currentSpeed = 0f;
        private float currentLocalX;

        public float Mass => mass;
        public float Gravity => gravity;
        public float RollingFriction => rollingFriction;
        public float TrackDistance => Mathf.Abs(endLocalX - startLocalX);
        public bool IsReleased => isReleased;
        public bool HasFinished => hasFinished;
        public float CurrentSpeed => currentSpeed;
        public float DistanceTraveled => Mathf.Max(0f, currentLocalX - startLocalX);
        public AdjustableRamp Ramp => ramp;

        private void Awake()
        {
            currentLocalX = startLocalX;
            ApplyCartLocalTransform();
        }

        private void Start()
        {
            if (ramp == null)
            {
                ramp = GetComponentInParent<AdjustableRamp>() ?? Object.FindFirstObjectByType<AdjustableRamp>();
            }
            if (ramp != null)
            {
                ramp.onAngleChanged.AddListener(OnRampAngleChanged);
            }
            ResetToInitialState();
        }

        private void OnDestroy()
        {
            if (ramp != null)
            {
                ramp.onAngleChanged.RemoveListener(OnRampAngleChanged);
            }
        }

        private void OnRampAngleChanged(float newAngle)
        {
            // Whenever the player changes the ramp angle, snap the cart back to the top start gate
            ResetToInitialState();
        }

        private void FixedUpdate()
        {
            if (!isReleased || hasFinished) return;

            float thetaRad = ramp != null ? ramp.AngleRadians : (20f * Mathf.Deg2Rad);
            float accel = gravity * Mathf.Max(0f, Mathf.Sin(thetaRad) - rollingFriction * Mathf.Cos(thetaRad));
            if (accel <= 0.0001f) return;

            float dt = Time.fixedDeltaTime;
            float prevSpeed = currentSpeed;
            float nextSpeed = prevSpeed + accel * dt;
            float avgSpeed = 0.5f * (prevSpeed + nextSpeed);
            float nextX = currentLocalX + avgSpeed * dt;

            if (nextX >= endLocalX)
            {
                // Exact kinematics at finish line: v_final = sqrt(v_prev^2 + 2 * a * remDist)
                float remDist = Mathf.Max(0f, endLocalX - currentLocalX);
                currentSpeed = Mathf.Sqrt(prevSpeed * prevSpeed + 2f * accel * remDist);
                currentLocalX = endLocalX;
                hasFinished = true;
                ApplyCartLocalTransform();
                onReachedBottom?.Invoke();
            }
            else
            {
                currentSpeed = nextSpeed;
                currentLocalX = nextX;
                ApplyCartLocalTransform();
            }
        }

        [ContextMenu("Release Cart")]
        public void Release()
        {
            if (isReleased) return;

            isReleased = true;
            hasFinished = false;
            currentSpeed = 0f;
            currentLocalX = startLocalX;
            ApplyCartLocalTransform();

            onReleased?.Invoke();
            float deg = ramp != null ? ramp.CurrentAngle : 20f;
            Debug.Log($"[InclinedCart] Released from rest on {deg:F0}° ramp.");
        }

        public void ResetToInitialState()
        {
            isReleased = false;
            hasFinished = false;
            currentSpeed = 0f;
            currentLocalX = startLocalX;
            ApplyCartLocalTransform();
        }

        private void ApplyCartLocalTransform()
        {
            transform.localPosition = new Vector3(currentLocalX, deckLocalY, 0f);
            transform.localRotation = Quaternion.identity;
        }
    }
}

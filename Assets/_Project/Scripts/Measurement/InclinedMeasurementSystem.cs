using System;
using UnityEngine;
using UnityEngine.Events;
using VRPhysicsLab.Core;
using VRPhysicsLab.Physics;

namespace VRPhysicsLab.Measurement
{
    [Serializable]
    public struct InclinedMeasurementResult
    {
        public float rampAngleDegrees;
        public float distanceTraveled;
        public float elapsedTime;
        public float finalVelocity;
        public float experimentalAcceleration;
        public float theoreticalAcceleration;
        public float parallelForce;
        public float relativeError;
    }

    /// <summary>
    /// Measures descent time, distance, final velocity, and acceleration for Station 3
    /// (Inclined Plane Experiment) and compares experimental a = 2d/t² against theoretical g*sin(theta).
    /// </summary>
    public class InclinedMeasurementSystem : MonoBehaviour, IResetable
    {
        [Header("References")]
        [SerializeField] private AdjustableRamp ramp;
        [SerializeField] private InclinedCart cart;

        [Header("Live State")]
        [SerializeField] private MeasurementState state = MeasurementState.Idle;
        [SerializeField] private float elapsedTime = 0f;
        [SerializeField] private float currentDistance = 0f;
        [SerializeField] private float currentSpeed = 0f;

        [Header("Events")]
        public UnityEvent onMeasurementStarted;
        public UnityEvent onMeasurementUpdated;
        public UnityEvent<InclinedMeasurementResult> onMeasurementCompleted;

        private InclinedMeasurementResult lastResult;

        public MeasurementState State => state;
        public float ElapsedTime => elapsedTime;
        public float CurrentDistance => currentDistance;
        public float CurrentSpeed => currentSpeed;
        public InclinedMeasurementResult LastResult => lastResult;

        private void Start()
        {
            if (ramp == null) ramp = UnityEngine.Object.FindFirstObjectByType<AdjustableRamp>();
            if (cart == null) cart = UnityEngine.Object.FindFirstObjectByType<InclinedCart>();

            if (cart != null)
            {
                cart.onReleased.AddListener(OnCartReleased);
                cart.onReachedBottom.AddListener(OnCartReachedBottom);
            }
            if (ramp != null)
            {
                ramp.onAngleChanged.AddListener(OnAngleChanged);
            }
        }

        private void OnDestroy()
        {
            if (cart != null)
            {
                cart.onReleased.RemoveListener(OnCartReleased);
                cart.onReachedBottom.RemoveListener(OnCartReachedBottom);
            }
            if (ramp != null)
            {
                ramp.onAngleChanged.RemoveListener(OnAngleChanged);
            }
        }

        private void FixedUpdate()
        {
            if (state == MeasurementState.Measuring && cart != null)
            {
                elapsedTime += Time.fixedDeltaTime;
                currentDistance = cart.DistanceTraveled;
                currentSpeed = cart.CurrentSpeed;
                onMeasurementUpdated?.Invoke();
            }
        }

        private void OnAngleChanged(float newAngle)
        {
            ResetToInitialState();
        }

        private void OnCartReleased()
        {
            state = MeasurementState.Measuring;
            elapsedTime = 0f;
            currentDistance = 0f;
            currentSpeed = 0f;
            onMeasurementStarted?.Invoke();
            onMeasurementUpdated?.Invoke();
        }

        private void OnCartReachedBottom()
        {
            if (state != MeasurementState.Measuring || cart == null) return;

            state = MeasurementState.Completed;

            float angleDeg = ramp != null ? ramp.CurrentAngle : 20f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float dist = cart.TrackDistance;
            float g = cart.Gravity;
            float m = cart.Mass;
            float mu = cart.RollingFriction;

            // Final velocity at bottom and exact elapsed time from v_final = a * t -> t = 2d / v_final
            float finalVel = cart.CurrentSpeed;
            float exactTime = finalVel > 0.001f ? (2f * dist / finalVel) : Mathf.Max(0.01f, elapsedTime);
            elapsedTime = exactTime;
            currentDistance = dist;
            currentSpeed = finalVel;

            float expAccel = (2f * dist) / (exactTime * exactTime);
            float theoAccel = g * Mathf.Max(0f, Mathf.Sin(angleRad) - mu * Mathf.Cos(angleRad));
            float parallelForce = m * g * Mathf.Sin(angleRad);
            float relError = theoAccel > 0.001f ? (Mathf.Abs(expAccel - theoAccel) / theoAccel * 100f) : 0f;

            lastResult = new InclinedMeasurementResult
            {
                rampAngleDegrees = angleDeg,
                distanceTraveled = dist,
                elapsedTime = exactTime,
                finalVelocity = finalVel,
                experimentalAcceleration = expAccel,
                theoreticalAcceleration = theoAccel,
                parallelForce = parallelForce,
                relativeError = relError
            };

            Debug.Log($"[InclinedMeasurementSystem] Run Finished! θ={angleDeg:F0}°, t={exactTime:F2}s, v={finalVel:F2}m/s, a_exp={expAccel:F2}m/s², a_theo={theoAccel:F2}m/s²");
            onMeasurementCompleted?.Invoke(lastResult);
            onMeasurementUpdated?.Invoke();
        }

        public void ResetToInitialState()
        {
            state = MeasurementState.Idle;
            elapsedTime = 0f;
            currentDistance = 0f;
            currentSpeed = 0f;
            onMeasurementUpdated?.Invoke();
        }
    }
}

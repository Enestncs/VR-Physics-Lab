using System;
using UnityEngine;
using UnityEngine.Events;
using VRPhysicsLab.Core;
using VRPhysicsLab.Physics;

namespace VRPhysicsLab.Measurement
{
    public enum MeasurementState
    {
        Idle,
        Measuring,
        Completed
    }

    [Serializable]
    public struct MeasurementData
    {
        public float elapsedTime;
        public float distanceTraveled;
        public float finalVelocity;
        public float averageAcceleration;
        public float mass;
        public float appliedForce;
    }

    /// <summary>
    /// Measures time, distance, velocity, and acceleration of the cart between start and end photogates.
    /// </summary>
    public class MeasurementSystem : MonoBehaviour, IResetable
    {
        [Header("References")]
        [SerializeField] private CartPhysics cart;
        [SerializeField] private ForceApplicator forceApplicator;
        [SerializeField] private GateSensor startGate;
        [SerializeField] private GateSensor endGate;

        [Header("Live Measurement Values")]
        [SerializeField] private MeasurementState state = MeasurementState.Idle;
        [SerializeField] private float elapsedTime = 0f;
        [SerializeField] private float distanceTraveled = 0f;
        [SerializeField] private float currentVelocity = 0f;
        [SerializeField] private float instantaneousAcceleration = 0f;
        [SerializeField] private float averageAcceleration = 0f;

        [Header("Events")]
        public UnityEvent onMeasurementStarted;
        public UnityEvent onMeasurementUpdated;
        public UnityEvent<MeasurementData> onMeasurementCompleted;

        private Vector3 startPosition;
        private float previousVelocity = 0f;
        private MeasurementData lastResult;

        public MeasurementState State => state;
        public float ElapsedTime => elapsedTime;
        public float DistanceTraveled => distanceTraveled;
        public float CurrentVelocity => currentVelocity;
        public float InstantaneousAcceleration => instantaneousAcceleration;
        public float AverageAcceleration => averageAcceleration;
        public MeasurementData LastResult => lastResult;

        private void Start()
        {
            if (startGate != null)
            {
                startGate.onCartTriggered.AddListener(OnGateTriggered);
            }
            if (endGate != null)
            {
                endGate.onCartTriggered.AddListener(OnGateTriggered);
            }
        }

        [Header("Braking & Track Boundaries")]
        [Tooltip("Deceleration rate when smoothly braking after passing the finish gate.")]
        [SerializeField] private float brakeDeceleration = 5.0f;

        [Tooltip("Maximum track X position (near end bumper) where cart will be safely stopped.")]
        [SerializeField] private float maxTrackX = 0.75f;

        private void FixedUpdate()
        {
            if (state == MeasurementState.Measuring && cart != null && cart.Rigidbody != null)
            {
                elapsedTime += Time.fixedDeltaTime;

                // Track distance along X axis
                distanceTraveled = Mathf.Abs(cart.transform.position.x - startPosition.x);

                // Speed
                currentVelocity = Mathf.Abs(cart.Rigidbody.linearVelocity.x);

                // Acceleration: a = delta_v / delta_t
                instantaneousAcceleration = (currentVelocity - previousVelocity) / Time.fixedDeltaTime;
                previousVelocity = currentVelocity;

                // Average acceleration: a = v / t (or 2*d / t^2)
                if (elapsedTime > 0.001f)
                {
                    averageAcceleration = currentVelocity / elapsedTime;
                }

                // Safety boundary: if cart reaches end of track without triggering end gate
                if (cart.transform.position.x >= maxTrackX)
                {
                    CompleteMeasurement();
                }

                onMeasurementUpdated?.Invoke();
            }
            else if (state == MeasurementState.Completed && cart != null && cart.Rigidbody != null)
            {
                // Smoothly decelerate cart to a safe stop at the end of the track
                Vector3 vel = cart.Rigidbody.linearVelocity;
                if (vel.sqrMagnitude > 0.0001f)
                {
                    cart.Rigidbody.linearVelocity = Vector3.MoveTowards(vel, Vector3.zero, brakeDeceleration * Time.fixedDeltaTime);
                }
                else
                {
                    cart.Rigidbody.linearVelocity = Vector3.zero;
                }

                // Clamp position so cart never rolls past the track end
                if (cart.transform.position.x > maxTrackX)
                {
                    Vector3 pos = cart.transform.position;
                    pos.x = maxTrackX;
                    cart.transform.position = pos;
                    cart.Rigidbody.linearVelocity = Vector3.zero;
                }
            }
        }

        private void OnGateTriggered(GateSensor gate, Collider other)
        {
            if (gate.Type == GateType.StartGate && state == MeasurementState.Idle)
            {
                StartMeasurement();
            }
            else if (gate.Type == GateType.EndGate && state == MeasurementState.Measuring)
            {
                CompleteMeasurement();
            }
        }

        /// <summary>
        /// Explicitly starts a measurement run.
        /// </summary>
        public void StartMeasurement()
        {
            if (cart == null) return;
            if (state != MeasurementState.Idle) return;

            state = MeasurementState.Measuring;
            elapsedTime = 0f;
            startPosition = cart.transform.position;
            distanceTraveled = 0f;
            currentVelocity = cart.Rigidbody != null ? Mathf.Abs(cart.Rigidbody.linearVelocity.x) : 0f;
            previousVelocity = currentVelocity;
            averageAcceleration = 0f;
            instantaneousAcceleration = 0f;

            onMeasurementStarted?.Invoke();
        }

        /// <summary>
        /// Completes the measurement run when the cart crosses the finish gate.
        /// </summary>
        public void CompleteMeasurement()
        {
            if (state != MeasurementState.Measuring) return;

            state = MeasurementState.Completed;

            // Stop applying force so cart decelerates or stops safely
            if (forceApplicator != null)
            {
                forceApplicator.StopApplyingForce();
            }

            // Calculate final averages
            if (elapsedTime > 0.001f)
            {
                averageAcceleration = currentVelocity / elapsedTime;
            }

            lastResult = new MeasurementData
            {
                elapsedTime = elapsedTime,
                distanceTraveled = distanceTraveled,
                finalVelocity = currentVelocity,
                averageAcceleration = averageAcceleration,
                mass = cart != null ? cart.TotalMass : 1f,
                appliedForce = forceApplicator != null ? forceApplicator.NetForce : 0f
            };

            Debug.Log($"[MeasurementSystem] Run Finished! Time: {elapsedTime:F2}s, Final Speed: {currentVelocity:F2}m/s, Avg Accel: {averageAcceleration:F2}m/s², Mass: {lastResult.mass}kg");
            onMeasurementCompleted?.Invoke(lastResult);
        }

        public void ResetToInitialState()
        {
            state = MeasurementState.Idle;
            elapsedTime = 0f;
            distanceTraveled = 0f;
            currentVelocity = 0f;
            instantaneousAcceleration = 0f;
            averageAcceleration = 0f;
            previousVelocity = 0f;
            onMeasurementUpdated?.Invoke();
        }
    }
}

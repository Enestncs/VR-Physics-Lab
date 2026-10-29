using System;
using UnityEngine;
using UnityEngine.Events;
using VRPhysicsLab.Core;
using VRPhysicsLab.Physics;

namespace VRPhysicsLab.Measurement
{
    [Serializable]
    public struct FrictionMeasurementResult
    {
        public string surfaceName;
        public float initialVelocity;
        public float stoppingDistance;
        public float stoppingTime;
        public float deceleration;
        public float experimentalMu;
        public float theoreticalMu;
        public float relativeError;
    }

    /// <summary>
    /// Measures stopping distance, stopping time, deceleration, and derived friction
    /// coefficient (\u03bc) for the surface friction experiment.
    /// </summary>
    public class FrictionMeasurementSystem : MonoBehaviour, IResetable
    {
        [Header("References")]
        [SerializeField] private FrictionCart cart;
        [SerializeField] private SurfacePlateSlot surfaceSlot;
        [SerializeField] private FrictionImpulseLauncher launcher;

        [Header("State")]
        [SerializeField] private MeasurementState state = MeasurementState.Idle;
        [SerializeField] private float elapsedTime = 0f;
        [SerializeField] private float currentDistance = 0f;
        [SerializeField] private float currentSpeed = 0f;
        [SerializeField] private float initialVelocity = 0f;

        [Header("Events")]
        public UnityEvent onMeasurementStarted;
        public UnityEvent onMeasurementUpdated;
        public UnityEvent<FrictionMeasurementResult> onMeasurementCompleted;

        private Vector3 startPosition;
        private FrictionMeasurementResult lastResult;
        private const float Gravity = 9.81f;

        public MeasurementState State => state;
        public float ElapsedTime => elapsedTime;
        public float CurrentDistance => currentDistance;
        public float CurrentSpeed => currentSpeed;
        public float InitialVelocity => initialVelocity;
        public FrictionMeasurementResult LastResult => lastResult;

        private void Start()
        {
            if (cart == null) cart = UnityEngine.Object.FindFirstObjectByType<FrictionCart>();
            if (surfaceSlot == null) surfaceSlot = UnityEngine.Object.FindFirstObjectByType<SurfacePlateSlot>();
            if (launcher == null) launcher = UnityEngine.Object.FindFirstObjectByType<FrictionImpulseLauncher>();

            if (launcher != null)
            {
                launcher.onLaunched.AddListener(OnLauncherFired);
            }
        }

        private void FixedUpdate()
        {
            if (state == MeasurementState.Measuring && cart != null && cart.Rigidbody != null)
            {
                elapsedTime += Time.fixedDeltaTime;
                currentDistance = Mathf.Abs(cart.transform.position.x - startPosition.x);
                currentSpeed = Mathf.Abs(cart.Rigidbody.linearVelocity.x);

                // Check if cart has come to a complete stop after launching
                if (elapsedTime > 0.15f && currentSpeed <= 0.005f)
                {
                    CompleteRun();
                }

                onMeasurementUpdated?.Invoke();
            }
        }

        private void OnLauncherFired()
        {
            StartRun();
        }

        public void StartRun()
        {
            if (cart == null) return;

            state = MeasurementState.Measuring;
            elapsedTime = 0f;
            startPosition = cart.transform.position;
            currentDistance = 0f;
            initialVelocity = launcher != null ? launcher.LaunchVelocity : (cart.Rigidbody != null ? Mathf.Abs(cart.Rigidbody.linearVelocity.x) : 1.2f);
            currentSpeed = initialVelocity;

            onMeasurementStarted?.Invoke();
        }

        public void CompleteRun()
        {
            if (state != MeasurementState.Measuring) return;

            state = MeasurementState.Completed;

            float stopDistance = Mathf.Max(0.01f, currentDistance);
            float stopTime = Mathf.Max(0.05f, elapsedTime);

            // Average deceleration: a = v_0 / t  or  v_0^2 / (2 * d)
            float decel = initialVelocity / stopTime;

            // Experimental \u03bc: \u03bc = v_0^2 / (2 * g * d)
            float expMu = (initialVelocity * initialVelocity) / (2f * Gravity * stopDistance);

            // Theoretical \u03bc from active surface plate
            float theoMu = 0.05f;
            string sName = "Standart";
            if (surfaceSlot != null && surfaceSlot.InstalledPlate != null)
            {
                theoMu = surfaceSlot.InstalledPlate.FrictionCoefficient;
                sName = surfaceSlot.InstalledPlate.DisplayName;
            }

            float relError = theoMu > 0.001f ? (Mathf.Abs(expMu - theoMu) / theoMu * 100f) : 0f;

            lastResult = new FrictionMeasurementResult
            {
                surfaceName = sName,
                initialVelocity = initialVelocity,
                stoppingDistance = stopDistance,
                stoppingTime = stopTime,
                deceleration = decel,
                experimentalMu = expMu,
                theoreticalMu = theoMu,
                relativeError = relError
            };

            Debug.Log($"[FrictionMeasurementSystem] Run Finished! Surface: {sName}, Distance: {stopDistance:F2}m, Time: {stopTime:F2}s, Decel: {decel:F2}m/s\u00b2, Exp \u03bc: {expMu:F3}, Theo \u03bc: {theoMu:F3}, Error: %{relError:F1}");
            onMeasurementCompleted?.Invoke(lastResult);
        }

        public void ResetToInitialState()
        {
            state = MeasurementState.Idle;
            elapsedTime = 0f;
            currentDistance = 0f;
            currentSpeed = 0f;
            initialVelocity = 0f;
            onMeasurementUpdated?.Invoke();
        }
    }
}

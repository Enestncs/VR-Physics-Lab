using UnityEngine;
using UnityEngine.Events;
using VRPhysicsLab.Core;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Delivers a reproducible, fixed initial velocity impulse (v_0) to the friction cart,
    /// ensuring fair, identical initial conditions for every tested surface plate.
    /// </summary>
    public class FrictionImpulseLauncher : MonoBehaviour, IResetable
    {
        [Header("Target & Impulse Settings")]
        [SerializeField] private FrictionCart targetCart;

        [Tooltip("Exact initial launch velocity along +X in m/s.")]
        [SerializeField] private float launchVelocity = 1.2f;

        [Header("Visual Feedback")]
        [SerializeField] private Transform pistonArm;
        [SerializeField] private Vector3 pushedOffset = new Vector3(0.04f, 0, 0);
        [SerializeField] private float returnSpeed = 6f;

        [Header("Events")]
        public UnityEvent onLaunched;

        private Vector3 initialPistonPos;
        private bool isLaunched = false;

        public float LaunchVelocity => launchVelocity;
        public bool IsLaunched => isLaunched;

        private void Start()
        {
            if (pistonArm != null)
            {
                initialPistonPos = pistonArm.localPosition;
            }
            if (targetCart == null)
            {
                targetCart = Object.FindFirstObjectByType<FrictionCart>();
            }
        }

        private void Update()
        {
            if (pistonArm != null)
            {
                pistonArm.localPosition = Vector3.Lerp(pistonArm.localPosition, initialPistonPos, Time.deltaTime * returnSpeed);
            }
        }

        [ContextMenu("Launch")]
        public void Launch()
        {
            if (targetCart == null)
            {
                targetCart = Object.FindFirstObjectByType<FrictionCart>();
            }

            if (targetCart == null) return;

            // Prevent re-launching after already launched until reset
            if (isLaunched || targetCart.IsLaunched)
            {
                return;
            }

            isLaunched = true;
            if (pistonArm != null)
            {
                pistonArm.localPosition = initialPistonPos + pushedOffset;
            }

            targetCart.Launch(launchVelocity);
            onLaunched?.Invoke();
            Debug.Log($"[FrictionImpulseLauncher] Cart launched with v_0 = {launchVelocity:F2} m/s");
        }

        public void ResetToInitialState()
        {
            isLaunched = false;
            if (pistonArm != null)
            {
                pistonArm.localPosition = initialPistonPos;
            }
        }
    }
}

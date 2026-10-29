using UnityEngine;
using UnityEngine.Events;
using VRPhysicsLab.Core;

namespace VRPhysicsLab.Physics
{
    public enum ForceApplicationMode
    {
        ContinuousConstantForce,
        InstantaneousImpulse
    }

    /// <summary>
    /// Applies a controlled, reproducible physical force to the cart along the track direction.
    /// Used to verify Newton's Second Law: a = F_net / m.
    /// </summary>
    public class ForceApplicator : MonoBehaviour, IResetable
    {
        [Header("Target & Configuration")]
        [Tooltip("The cart to which force will be applied.")]
        [SerializeField] private CartPhysics targetCart;

        [Tooltip("Direction in which force is applied in world space.")]
        [SerializeField] private Vector3 forceDirection = Vector3.right;

        [Tooltip("Magnitude of the net force in Newtons.")]
        [SerializeField] private float netForce = 4.0f;

        [Tooltip("Mode of force application.")]
        [SerializeField] private ForceApplicationMode forceMode = ForceApplicationMode.ContinuousConstantForce;

        [Header("State")]
        [SerializeField] private bool isApplyingForce = false;
        [SerializeField] private bool hasLaunched = false;

        [Header("Events")]
        public UnityEvent onForceStarted;
        public UnityEvent onForceStopped;

        public float NetForce
        {
            get => netForce;
            set => netForce = Mathf.Max(0f, value);
        }

        public bool IsApplyingForce => isApplyingForce;
        public bool HasLaunched => hasLaunched;
        public ForceApplicationMode Mode => forceMode;

        private void FixedUpdate()
        {
            if (isApplyingForce && targetCart != null && targetCart.Rigidbody != null)
            {
                if (forceMode == ForceApplicationMode.ContinuousConstantForce)
                {
                    // F = m * a  => ForceMode.Force applies continuous force in Newtons
                    targetCart.Rigidbody.AddForce(forceDirection.normalized * netForce, ForceMode.Force);
                }
            }
        }

        /// <summary>
        /// Starts applying net force to the target cart.
        /// Once launched, cannot be triggered again until ResetToInitialState() is called.
        /// </summary>
        [ContextMenu("Start Force")]
        public void StartApplyingForce()
        {
            if (targetCart == null) return;

            // Prevent re-launching after already launched until reset
            if (hasLaunched || isApplyingForce)
            {
                return;
            }

            hasLaunched = true;
            isApplyingForce = true;
            onForceStarted?.Invoke();

            if (forceMode == ForceApplicationMode.InstantaneousImpulse && targetCart.Rigidbody != null)
            {
                targetCart.Rigidbody.AddForce(forceDirection.normalized * netForce, ForceMode.Impulse);
                isApplyingForce = false;
                onForceStopped?.Invoke();
            }
        }

        /// <summary>
        /// Stops applying force to the cart.
        /// </summary>
        [ContextMenu("Stop Force")]
        public void StopApplyingForce()
        {
            if (isApplyingForce)
            {
                isApplyingForce = false;
                onForceStopped?.Invoke();
            }
        }

        public void SetTargetCart(CartPhysics cart)
        {
            targetCart = cart;
        }

        public void ResetToInitialState()
        {
            hasLaunched = false;
            StopApplyingForce();
        }
    }
}

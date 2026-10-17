using UnityEngine;
using UnityEngine.Events;
using BNG;

namespace VRPhysicsLab.Core
{
    /// <summary>
    /// Remembers the initial transform, rigidbody state, and grabbable state of an object
    /// and allows instant resetting to that initial state.
    /// </summary>
    [DisallowMultipleComponent]
    public class ResetableObject : MonoBehaviour, IResetable
    {
        [Header("Settings")]
        [Tooltip("If true, saves transform and physics state on Start().")]
        [SerializeField] private bool autoRecordOnStart = true;

        [Tooltip("If true, resets velocity and angular velocity to zero on reset.")]
        [SerializeField] private bool resetVelocities = true;

        [Header("Events")]
        public UnityEvent onReset;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 initialScale;
        private Transform initialParent;

        private Rigidbody rb;
        private float initialMass;
        private bool initialUseGravity;
        private bool initialIsKinematic;

        private Grabbable grabbable;
        private bool hasRecorded = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            grabbable = GetComponent<Grabbable>();
        }

        private void Start()
        {
            if (autoRecordOnStart && !hasRecorded)
            {
                RecordInitialState();
            }
        }

        /// <summary>
        /// Explicitly records the current transform and physics state as the reset target.
        /// </summary>
        public void RecordInitialState()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            initialScale = transform.localScale;
            initialParent = transform.parent;

            if (rb != null)
            {
                initialMass = rb.mass;
                initialUseGravity = rb.useGravity;
                initialIsKinematic = rb.isKinematic;
            }

            hasRecorded = true;
        }

        /// <summary>
        /// Resets the object back to its recorded initial state.
        /// </summary>
        public void ResetToInitialState()
        {
            if (!hasRecorded)
            {
                RecordInitialState();
            }

            // If object is currently held in VR, force drop it safely
            if (grabbable != null && grabbable.BeingHeld)
            {
                if (grabbable.HeldByGrabbers != null && grabbable.HeldByGrabbers.Count > 0)
                {
                    for (int i = grabbable.HeldByGrabbers.Count - 1; i >= 0; i--)
                    {
                        var grabber = grabbable.HeldByGrabbers[i];
                        if (grabber != null)
                        {
                            grabbable.DropItem(grabber, false, true);
                        }
                    }
                }
            }

            // Restore parent if changed
            if (transform.parent != initialParent)
            {
                transform.SetParent(initialParent);
            }

            // Restore transform
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            transform.localScale = initialScale;

            // Reset Rigidbody physics
            if (rb != null)
            {
                if (resetVelocities)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.mass = initialMass;
                rb.useGravity = initialUseGravity;
                rb.isKinematic = initialIsKinematic;
            }

            onReset?.Invoke();
        }
    }
}

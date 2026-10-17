using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VRPhysicsLab.Core;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Controls the physical properties, mass stacking, and track motion constraints of the physics cart.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CartPhysics : MonoBehaviour, IResetable
    {
        [Header("Mass Configuration")]
        [Tooltip("Base mass of the empty cart in kg.")]
        [SerializeField] private float baseMass = 1.0f;

        [Tooltip("Current total mass (base mass + attached weights).")]
        [SerializeField] private float currentTotalMass = 1.0f;

        [Header("Track Constraints")]
        [Tooltip("If true, constrains motion to the X-axis while on the track.")]
        [SerializeField] private bool constrainToTrack = true;

        [Header("Events")]
        public UnityEvent<float> onMassChanged;

        private Rigidbody rb;
        private readonly List<Rigidbody> attachedWeights = new List<Rigidbody>();

        public float TotalMass => currentTotalMass;
        public Rigidbody Rigidbody
        {
            get
            {
                if (rb == null) rb = GetComponent<Rigidbody>();
                return rb;
            }
        }
        public float BaseMass => baseMass;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            UpdateTotalMass();
        }

        private void Start()
        {
            ApplyTrackConstraints();
        }

        /// <summary>
        /// Constrains the cart to glide purely along the track's X-axis without tipping or sliding off.
        /// </summary>
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
        /// Removes track constraints, allowing free 3D motion (e.g. if picked up in VR).
        /// </summary>
        public void RemoveTrackConstraints()
        {
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None;
            }
        }

        /// <summary>
        /// Registers a weight block placed onto the cart.
        /// </summary>
        public void AddWeight(Rigidbody weightRb)
        {
            if (weightRb != null && !attachedWeights.Contains(weightRb))
            {
                attachedWeights.Add(weightRb);
                UpdateTotalMass();
            }
        }

        /// <summary>
        /// Unregisters a weight block removed from the cart.
        /// </summary>
        public void RemoveWeight(Rigidbody weightRb)
        {
            if (weightRb != null && attachedWeights.Contains(weightRb))
            {
                attachedWeights.Remove(weightRb);
                UpdateTotalMass();
            }
        }

        /// <summary>
        /// Recalculates total mass from base mass and all attached weight blocks.
        /// </summary>
        public void UpdateTotalMass()
        {
            float total = baseMass;
            for (int i = attachedWeights.Count - 1; i >= 0; i--)
            {
                if (attachedWeights[i] != null)
                {
                    total += attachedWeights[i].mass;
                }
                else
                {
                    attachedWeights.RemoveAt(i);
                }
            }

            currentTotalMass = total;
            if (rb != null)
            {
                rb.mass = currentTotalMass;
            }

            onMassChanged?.Invoke(currentTotalMass);
        }

        /// <summary>
        /// Resets the cart to base mass and reapplies track constraints.
        /// </summary>
        public void ResetToInitialState()
        {
            attachedWeights.Clear();
            UpdateTotalMass();
            ApplyTrackConstraints();
        }
    }
}

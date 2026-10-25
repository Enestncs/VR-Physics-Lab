using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BNG;
using VRPhysicsLab.Core;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Controls the physical properties, mass stacking, and track motion constraints of the physics cart.
    /// Weights placed on the cart are locked to it (kinematic + parented) so they ride along without
    /// sliding on the frictionless cart surface; their mass is added to the cart's Rigidbody instead.
    /// Grabbing a locked weight in VR automatically releases it.
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

        private class AttachedWeight
        {
            public Rigidbody rb;
            public Grabbable grabbable;
            public Transform originalParent;
            public bool wasKinematic;
            public RigidbodyInterpolation interpolation;
            public Collider[] colliders;
        }

        private Rigidbody rb;
        private readonly List<AttachedWeight> attachedWeights = new List<AttachedWeight>();
        private Collider[] cartColliders;

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
        public int AttachedWeightCount => attachedWeights.Count;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            CacheCartColliders();
            UpdateTotalMass();
        }

        private void Start()
        {
            ApplyTrackConstraints();
        }

        private void CacheCartColliders()
        {
            // Only solid colliders: ignoring trigger colliders would also stop trigger events (weight slot)
            var all = GetComponentsInChildren<Collider>(true);
            var solid = new List<Collider>(all.Length);
            foreach (var c in all)
            {
                if (c != null && !c.isTrigger && c.attachedRigidbody == Rigidbody)
                {
                    solid.Add(c);
                }
            }
            cartColliders = solid.ToArray();
        }

        private void FixedUpdate()
        {
            // Release weights the player grabs off the cart
            for (int i = attachedWeights.Count - 1; i >= 0; i--)
            {
                var w = attachedWeights[i];
                if (w.rb == null)
                {
                    attachedWeights.RemoveAt(i);
                    UpdateTotalMass();
                    continue;
                }

                if (w.grabbable != null && w.grabbable.BeingHeld)
                {
                    DetachWeight(w.rb);
                }
            }
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

        public bool IsWeightAttached(Rigidbody weightRb)
        {
            return IndexOf(weightRb) >= 0;
        }

        private int IndexOf(Rigidbody weightRb)
        {
            for (int i = 0; i < attachedWeights.Count; i++)
            {
                if (attachedWeights[i].rb == weightRb) return i;
            }
            return -1;
        }

        /// <summary>
        /// Locks a weight block onto the cart so it moves with it and adds its mass.
        /// </summary>
        public bool AttachWeight(Rigidbody weightRb)
        {
            if (weightRb == null || weightRb == Rigidbody || IsWeightAttached(weightRb)) return false;

            var data = new AttachedWeight
            {
                rb = weightRb,
                grabbable = weightRb.GetComponent<Grabbable>(),
                originalParent = weightRb.transform.parent,
                wasKinematic = weightRb.isKinematic,
                interpolation = weightRb.interpolation,
                colliders = weightRb.GetComponentsInChildren<Collider>()
            };

            if (!weightRb.isKinematic)
            {
                weightRb.linearVelocity = Vector3.zero;
                weightRb.angularVelocity = Vector3.zero;
            }
            weightRb.isKinematic = true;
            weightRb.interpolation = RigidbodyInterpolation.None; // follows the (interpolated) cart transform
            weightRb.transform.SetParent(transform, true);

            SetCollisionIgnored(data, true);

            attachedWeights.Add(data);
            UpdateTotalMass();
            return true;
        }

        /// <summary>
        /// Releases a weight block from the cart and restores its free physics.
        /// </summary>
        public bool DetachWeight(Rigidbody weightRb)
        {
            int index = IndexOf(weightRb);
            if (index < 0) return false;

            var data = attachedWeights[index];
            attachedWeights.RemoveAt(index);

            if (weightRb != null)
            {
                SetCollisionIgnored(data, false);

                if (weightRb.transform.parent == transform)
                {
                    weightRb.transform.SetParent(data.originalParent, true);
                }

                weightRb.isKinematic = data.wasKinematic;
                weightRb.interpolation = data.interpolation;

                if (!weightRb.isKinematic && Rigidbody != null)
                {
                    // Keep the cart's momentum so it doesn't look like it stopped dead
                    weightRb.linearVelocity = Rigidbody.linearVelocity;
                }
            }

            UpdateTotalMass();
            return true;
        }

        private void SetCollisionIgnored(AttachedWeight data, bool ignore)
        {
            if (data.colliders == null || cartColliders == null) return;

            foreach (var wc in data.colliders)
            {
                if (wc == null) continue;
                foreach (var cc in cartColliders)
                {
                    if (cc == null) continue;
                    UnityEngine.Physics.IgnoreCollision(wc, cc, ignore);
                }
            }
        }

        /// <summary>
        /// Registers a weight block placed onto the cart. (Kept for backwards compatibility.)
        /// </summary>
        public void AddWeight(Rigidbody weightRb)
        {
            AttachWeight(weightRb);
        }

        /// <summary>
        /// Unregisters a weight block removed from the cart. (Kept for backwards compatibility.)
        /// </summary>
        public void RemoveWeight(Rigidbody weightRb)
        {
            DetachWeight(weightRb);
        }

        /// <summary>
        /// Recalculates total mass from base mass and all attached weight blocks.
        /// </summary>
        public void UpdateTotalMass()
        {
            float total = baseMass;
            for (int i = attachedWeights.Count - 1; i >= 0; i--)
            {
                if (attachedWeights[i].rb != null)
                {
                    total += attachedWeights[i].rb.mass;
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
            for (int i = attachedWeights.Count - 1; i >= 0; i--)
            {
                DetachWeight(attachedWeights[i].rb);
            }
            attachedWeights.Clear();
            GetComponentInChildren<CartWeightSlot>()?.ClearCandidates();
            UpdateTotalMass();
            ApplyTrackConstraints();
        }
    }
}

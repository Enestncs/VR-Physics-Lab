using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BNG;
using VRPhysicsLab.Core;
using VRPhysicsLab.Measurement;

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

        [Header("Tray Slot Configuration")]
        [Tooltip("Local Y position of the tray floor relative to the cart root.")]
        [SerializeField] private float trayFloorLocalY = 0.0265f;

        [Tooltip("Slot local X offsets when multiple weights are placed side-by-side.")]
        [SerializeField] private float[] slotXOffsets = new float[] { -0.05f, 0.05f };

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
        private Collider[] trackColliders;
        private MeasurementSystem measurementSystem;

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
            CacheTrackColliders();
            UpdateTotalMass();
        }

        private void Start()
        {
            ApplyTrackConstraints();
            if (measurementSystem == null)
            {
                measurementSystem = Object.FindFirstObjectByType<MeasurementSystem>();
            }
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

        private void CacheTrackColliders()
        {
            var trackObj = GameObject.Find("Track_Assembly") ?? GameObject.Find("Track");
            if (trackObj != null)
            {
                var cols = trackObj.GetComponentsInChildren<Collider>(true);
                var solid = new List<Collider>();
                foreach (var c in cols)
                {
                    if (c != null && !c.isTrigger) solid.Add(c);
                }
                trackColliders = solid.ToArray();
            }
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
                    RearrangeWeights();
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

            // 1. Zero out any residual velocities
            if (!weightRb.isKinematic)
            {
                weightRb.linearVelocity = Vector3.zero;
                weightRb.angularVelocity = Vector3.zero;
            }

            // 2. Lock to kinematic before parenting to prevent physics impulses
            weightRb.isKinematic = true;
            weightRb.interpolation = RigidbodyInterpolation.None;
            weightRb.transform.SetParent(transform, true);

            // 3. Ignore collisions with cart, existing attached weights, and track
            SetCollisionIgnored(data, true);

            // 4. Register and rearrange weights neatly in the tray
            attachedWeights.Add(data);
            RearrangeWeights();

            // 5. If experiment is idle, ensure cart remains completely stationary
            if (measurementSystem == null) measurementSystem = Object.FindFirstObjectByType<MeasurementSystem>();
            if (measurementSystem == null || measurementSystem.State == MeasurementState.Idle)
            {
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

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
                    // Only transfer velocity if cart was actually in motion
                    if (Rigidbody.linearVelocity.sqrMagnitude > 0.01f)
                    {
                        weightRb.linearVelocity = Rigidbody.linearVelocity;
                    }
                    else
                    {
                        weightRb.linearVelocity = Vector3.zero;
                    }
                }
            }

            RearrangeWeights();
            UpdateTotalMass();
            return true;
        }

        /// <summary>
        /// Ignores or restores collisions between the given weight and:
        /// 1. The cart's own colliders
        /// 2. All other attached weights on the cart
        /// 3. Track rails/colliders
        /// </summary>
        private void SetCollisionIgnored(AttachedWeight data, bool ignore)
        {
            if (data.colliders == null) return;

            // 1. Cart colliders
            if (cartColliders != null)
            {
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

            // 2. All other attached weights
            for (int i = 0; i < attachedWeights.Count; i++)
            {
                var other = attachedWeights[i];
                if (other == null || other == data || other.colliders == null) continue;

                foreach (var wc in data.colliders)
                {
                    if (wc == null) continue;
                    foreach (var oc in other.colliders)
                    {
                        if (oc == null) continue;
                        UnityEngine.Physics.IgnoreCollision(wc, oc, ignore);
                    }
                }
            }

            // 3. Track colliders
            if (trackColliders != null)
            {
                foreach (var wc in data.colliders)
                {
                    if (wc == null) continue;
                    foreach (var tc in trackColliders)
                    {
                        if (tc == null) continue;
                        UnityEngine.Physics.IgnoreCollision(wc, tc, ignore);
                    }
                }
            }
        }

        /// <summary>
        /// Ignores or restores collision between external colliders (e.g. candidate weights in hand) and cart colliders.
        /// Prevents VR hand from pushing the cart while hovering over the tray.
        /// </summary>
        public void SetCandidateCollisionIgnored(Collider[] colliders, bool ignore)
        {
            if (colliders == null || cartColliders == null) return;

            foreach (var wc in colliders)
            {
                if (wc == null) continue;
                foreach (var cc in cartColliders)
                {
                    if (cc == null) continue;
                    UnityEngine.Physics.IgnoreCollision(wc, cc, ignore);
                }
                // Also ignore with already attached weights
                foreach (var aw in attachedWeights)
                {
                    if (aw.colliders == null) continue;
                    foreach (var ac in aw.colliders)
                    {
                        if (ac == null) continue;
                        UnityEngine.Physics.IgnoreCollision(wc, ac, ignore);
                    }
                }
            }
        }

        /// <summary>
        /// Rearranges all attached weights so they sit balanced and upright in the cart tray with zero clipping.
        /// </summary>
        private void RearrangeWeights()
        {
            if (attachedWeights.Count == 1)
            {
                var w = attachedWeights[0];
                if (w != null && w.rb != null)
                {
                    float y = trayFloorLocalY + GetWeightHalfHeight(w.rb);
                    w.rb.transform.localPosition = new Vector3(0f, y, 0f);
                    w.rb.transform.localRotation = Quaternion.identity;
                }
            }
            else if (attachedWeights.Count >= 2)
            {
                for (int i = 0; i < attachedWeights.Count; i++)
                {
                    var w = attachedWeights[i];
                    if (w == null || w.rb == null) continue;

                    float slotX = (i < slotXOffsets.Length) ? slotXOffsets[i] : (i == 0 ? -0.05f : 0.05f);
                    float y = trayFloorLocalY + GetWeightHalfHeight(w.rb);
                    w.rb.transform.localPosition = new Vector3(slotX, y, 0f);
                    w.rb.transform.localRotation = Quaternion.identity;
                }
            }

            UnityEngine.Physics.SyncTransforms();
        }

        private float GetWeightHalfHeight(Rigidbody weightRb)
        {
            var box = weightRb.GetComponentInChildren<BoxCollider>();
            if (box != null)
            {
                return box.size.y * weightRb.transform.lossyScale.y * 0.5f;
            }
            var col = weightRb.GetComponentInChildren<Collider>();
            if (col != null)
            {
                return col.bounds.extents.y;
            }
            return 0.04f;
        }

        public void AddWeight(Rigidbody weightRb) => AttachWeight(weightRb);
        public void RemoveWeight(Rigidbody weightRb) => DetachWeight(weightRb);

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

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}

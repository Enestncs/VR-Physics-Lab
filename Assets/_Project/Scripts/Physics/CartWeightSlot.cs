using System.Collections.Generic;
using UnityEngine;
using BNG;

namespace VRPhysicsLab.Physics
{
    /// <summary>
    /// Trigger zone on top of the physics cart that detects, locks, and registers placed weight blocks.
    /// Tracks candidates inside the trigger volume:
    /// - Only accepts valid weight objects (objects with a Grabbable component that are NOT the player/hands).
    /// - If a weight is held by the player in VR, it remains detached so the player can place it freely.
    /// - As soon as the player releases/drops the weight inside the slot, it is locked onto the cart
    ///   (parented + kinematic + mass added) so it NEVER slides off the frictionless cart bed.
    /// - If the player grabs an attached weight, it is instantly detached and freed back to the hand.
    /// - Attached weights are NEVER detached by trigger jitter when not held.
    /// </summary>
    public class CartWeightSlot : MonoBehaviour
    {
        [SerializeField] private CartPhysics cartPhysics;

        private class CandidateWeight
        {
            public Rigidbody rb;
            public Grabbable grabbable;
            public Collider[] colliders;
        }

        private readonly List<CandidateWeight> candidates = new List<CandidateWeight>();

        private void Awake()
        {
            if (cartPhysics == null)
            {
                cartPhysics = GetComponentInParent<CartPhysics>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (cartPhysics == null) return;

            var rb = other.attachedRigidbody;
            if (rb == null || rb == cartPhysics.Rigidbody) return;

            // CRITICAL: Strictly reject player hands, grabbers, controllers or the player body!
            if (other.GetComponentInParent<Grabber>() != null ||
                other.GetComponentInParent<BNGPlayerController>() != null ||
                other.GetComponentInParent<HandController>() != null ||
                rb.GetComponentInParent<Grabber>() != null ||
                rb.GetComponentInParent<BNGPlayerController>() != null)
            {
                return;
            }

            // Only grabbable weight items can be attached
            var grabbable = rb.GetComponent<Grabbable>();
            if (grabbable == null) return;

            // Don't add duplicate entries for multi-collider weights
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].rb == rb) return;
            }

            var colliders = rb.GetComponentsInChildren<Collider>();
            var candidate = new CandidateWeight
            {
                rb = rb,
                grabbable = grabbable,
                colliders = colliders
            };

            // Ignore collision between hovering candidate weight and cart so VR hand never pushes the cart
            cartPhysics.SetCandidateCollisionIgnored(colliders, true);

            candidates.Add(candidate);
        }

        private void OnTriggerExit(Collider other)
        {
            if (cartPhysics == null) return;

            var rb = other.attachedRigidbody;
            if (rb == null || rb == cartPhysics.Rigidbody) return;

            bool isAttached = cartPhysics.IsWeightAttached(rb);
            var grabbable = rb.GetComponent<Grabbable>();
            bool isHeld = grabbable != null && grabbable.BeingHeld;

            // CRITICAL FIX: If the weight is attached to the cart and NOT held by player,
            // DO NOT detach it! Keeps the weight locked onto the cart and eliminates trigger-flicker loops.
            if (isAttached && !isHeld)
            {
                return;
            }

            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                if (candidates[i].rb == rb)
                {
                    cartPhysics.SetCandidateCollisionIgnored(candidates[i].colliders, false);
                    candidates.RemoveAt(i);
                    break;
                }
            }

            if (isAttached)
            {
                cartPhysics.DetachWeight(rb);
            }
        }

        private void FixedUpdate()
        {
            if (cartPhysics == null) return;

            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                var candidate = candidates[i];
                if (candidate == null || candidate.rb == null)
                {
                    candidates.RemoveAt(i);
                    continue;
                }

                bool isHeld = candidate.grabbable != null && candidate.grabbable.BeingHeld;
                bool isAttached = cartPhysics.IsWeightAttached(candidate.rb);

                if (isHeld)
                {
                    // Player is holding the weight: detach it so VR hand physics take full control
                    if (isAttached)
                    {
                        cartPhysics.DetachWeight(candidate.rb);
                    }
                }
                else
                {
                    // Player has released/dropped the weight into the tray: lock it onto the cart!
                    if (!isAttached)
                    {
                        cartPhysics.AttachWeight(candidate.rb);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all candidate weights when resetting.
        /// </summary>
        public void ClearCandidates()
        {
            foreach (var c in candidates)
            {
                if (c != null && c.colliders != null && cartPhysics != null)
                {
                    cartPhysics.SetCandidateCollisionIgnored(c.colliders, false);
                }
            }
            candidates.Clear();
        }
    }
}
